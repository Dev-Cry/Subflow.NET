using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ruleflow.NET.Engine.Validation;
using Ruleflow.NET.Engine.Validation.Core.Base;
using Ruleflow.NET.Engine.Validation.Core.Context;
using Ruleflow.NET.Engine.Validation.Core.Results;
using Ruleflow.NET.Engine.Validation.Core.Validators;
using Ruleflow.NET.Engine.Validation.Enums;
using Ruleflow.NET.Engine.Validation.Interfaces;
using System;
using System.Collections.Generic;

namespace Ruleflow.NET.Tests
{
    [TestClass]
    public class ValidationContextTests
    {
        private class ShoppingCart
        {
            public int Id { get; set; }
            public List<CartItem> Items { get; set; } = new List<CartItem>();
            public string PromoCode { get; set; }
            public decimal TotalAmount { get; set; }
            public string CustomerId { get; set; }
            public bool HasShippingAddress { get; set; }
        }

        private class CartItem
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; }
            public decimal Price { get; set; }
            public int Quantity { get; set; }
        }

        [TestMethod]
        public void ValidationContext_PropagatesProperty_BetweenRules()
        {
            // Arrange
            // First rule checks cart items and saves calculated total to context
            var calculateTotalRule = RuleflowExtensions.CreateRule<ShoppingCart>()
                .WithId("CalculateTotalRule")
                .WithAction(cart => {
                    decimal calculatedTotal = 0;
                    foreach (var item in cart.Items)
                    {
                        calculatedTotal += item.Price * item.Quantity;
                    }

                    // Store calculated total in context for other rules to use
                    var context = new ValidationContext();
                    context.Properties["CalculatedTotal"] = calculatedTotal;

                    // Verify total matches the cart's total
                    if (Math.Abs(calculatedTotal - cart.TotalAmount) > 0.01m)
                    {
                        throw new ArgumentException($"Cart total mismatch. Expected: {calculatedTotal}, Actual: {cart.TotalAmount}");
                    }
                })
                .Build();

            // Testing a rule that depends on the context property but doesn't have a formal dependency
            var contextAccessRule = new ContextAwareTestRule("ContextAccessRule");

            var validator = new DependencyAwareValidator<ShoppingCart>(
                new IValidationRule<ShoppingCart>[] { calculateTotalRule, contextAccessRule });

            // Create a cart with correct total
            var cart = new ShoppingCart
            {
                Id = 1,
                Items = new List<CartItem>
                {
                    new CartItem { ProductId = 101, ProductName = "Product 1", Price = 10.99m, Quantity = 2 },
                    new CartItem { ProductId = 102, ProductName = "Product 2", Price = 24.99m, Quantity = 1 }
                },
                TotalAmount = 46.97m  // 10.99*2 + 24.99
            };

            // Act
            var result = validator.CollectValidationResults(cart);

            // Assert
            Assert.IsTrue(result.IsValid);
            Assert.IsTrue(contextAccessRule.ContextAccessSuccessful, "Context property was not accessible");
        }

        [TestMethod]
        public void ValidationContext_PropagatesRuleResults_ToOtherRules()
        {
            // Arrange
            // First rule checks if cart has items
            var hasItemsRule = RuleflowExtensions.CreateRule<ShoppingCart>()
                .WithId("HasItemsRule")
                .WithAction(cart => {
                    if (cart.Items.Count == 0)
                        throw new ArgumentException("Cart has no items");
                })
                .Build();

            // Second rule checks if promo code is valid
            var promoCodeRule = RuleflowExtensions.CreateRule<ShoppingCart>()
                .WithId("PromoCodeRule")
                .WithAction(cart => {
                    if (string.IsNullOrEmpty(cart.PromoCode))
                        return; // No promo code is valid

                    if (cart.PromoCode != "VALID10" && cart.PromoCode != "VALID20")
                        throw new ArgumentException("Invalid promo code");
                })
                .Build();

            // Third rule checks if both previous rules have succeeded
            var resultCheckRule = new RuleResultCheckingRule("ResultCheckRule",
                new[] { "HasItemsRule", "PromoCodeRule" });

            var validator = new DependencyAwareValidator<ShoppingCart>(
                new IValidationRule<ShoppingCart>[] { hasItemsRule, promoCodeRule, resultCheckRule });

            // Cart with items and valid promo code
            var validCart = new ShoppingCart
            {
                Id = 1,
                Items = new List<CartItem> { new CartItem { ProductId = 101, ProductName = "Product 1", Price = 10.99m, Quantity = 1 } },
                PromoCode = "VALID10"
            };

            // Act
            var result = validator.CollectValidationResults(validCart);

            // Assert
            Assert.IsTrue(result.IsValid);
            Assert.IsTrue(resultCheckRule.AllRulesSucceeded, "Rule results were not accessible in context");
        }

        [TestMethod]
        public void ValidationContext_AllowsRules_ToCheckFailedDependencies()
        {
            // Arrange
            // Rule that will fail
            var cartValidRule = RuleflowExtensions.CreateRule<ShoppingCart>()
                .WithId("CartValidRule")
                .WithAction(cart => {
                    throw new ArgumentException("Cart validation failed");
                })
                .Build();

            // Rule that checks if the first rule failed
            var failureCheckRule = new RuleFailureCheckingRule("FailureCheckRule", "CartValidRule");

            var validator = new DependencyAwareValidator<ShoppingCart>(
                new IValidationRule<ShoppingCart>[] { cartValidRule, failureCheckRule });

            var cart = new ShoppingCart { Id = 1 };

            // Act
            var result = validator.CollectValidationResults(cart);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("Cart validation failed", result.Errors[0].Message);
            Assert.IsTrue(failureCheckRule.FailureDetected, "Rule failure was not accessible in context");
        }

        [TestMethod]
        public void ValidationContext_CanStoreAndRetrieveCustomProperties()
        {
            // Arrange
            // Create a custom validation context
            var context = new ValidationContext();

            // Add various property types
            context.Properties["StringValue"] = "Test String";
            context.Properties["IntValue"] = 42;
            context.Properties["DateValue"] = new DateTime(2025, 4, 24);
            context.Properties["ListValue"] = new List<string> { "One", "Two", "Three" };
            context.Properties["ObjectValue"] = new CartItem { ProductId = 101, Price = 19.99m };

            // Act & Assert
            // Verify property retrieval
            Assert.AreEqual("Test String", context.Properties["StringValue"]);
            Assert.AreEqual(42, context.Properties["IntValue"]);
            Assert.AreEqual(new DateTime(2025, 4, 24), context.Properties["DateValue"]);

            var retrievedList = context.Properties["ListValue"] as List<string>;
            Assert.IsNotNull(retrievedList);
            Assert.AreEqual(3, retrievedList.Count);
            Assert.AreEqual("Two", retrievedList[1]);

            var retrievedObject = context.Properties["ObjectValue"] as CartItem;
            Assert.IsNotNull(retrievedObject);
            Assert.AreEqual(101, retrievedObject.ProductId);
            Assert.AreEqual(19.99m, retrievedObject.Price);
        }

        [TestMethod]
        public void ValidationContext_HandlesNonExistentProperties_Gracefully()
        {
            // Arrange
            var context = new ValidationContext();
            context.Properties["ExistingKey"] = "Value";

            // Act & Assert
            // Accessing non-existent key should throw KeyNotFoundException
            Assert.ThrowsException<KeyNotFoundException>(() => {
                var value = context.Properties["NonExistentKey"];
            });

            // Existing key should work
            Assert.AreEqual("Value", context.Properties["ExistingKey"]);
        }

        [TestMethod]
        public void ValidationContext_GetErrorsBySeverity_FiltersCorrectedlyByLevel()
        {
            // Arrange
            // Create rules with different severity levels
            var verboseRule = RuleflowExtensions.CreateRule<ShoppingCart>()
                .WithId("VerboseRule")
                .WithSeverity(ValidationSeverity.Verbose)
                .WithAction(cart => {
                    throw new ArgumentException("Verbose level message");
                })
                .Build();

            var infoRule = RuleflowExtensions.CreateRule<ShoppingCart>()
                .WithId("InfoRule")
                .WithSeverity(ValidationSeverity.Information)
                .WithAction(cart => {
                    throw new ArgumentException("Information level message");
                })
                .Build();

            var warningRule = RuleflowExtensions.CreateRule<ShoppingCart>()
                .WithId("WarningRule")
                .WithSeverity(ValidationSeverity.Warning)
                .WithAction(cart => {
                    throw new ArgumentException("Warning level message");
                })
                .Build();

            var errorRule = RuleflowExtensions.CreateRule<ShoppingCart>()
                .WithId("ErrorRule")
                .WithSeverity(ValidationSeverity.Error)
                .WithAction(cart => {
                    throw new ArgumentException("Error level message");
                })
                .Build();

            var criticalRule = RuleflowExtensions.CreateRule<ShoppingCart>()
                .WithId("CriticalRule")
                .WithSeverity(ValidationSeverity.Critical)
                .WithAction(cart => {
                    throw new ArgumentException("Critical level message");
                })
                .Build();

            var validator = new DependencyAwareValidator<ShoppingCart>(
                new[] { verboseRule, infoRule, warningRule, errorRule, criticalRule });

            var cart = new ShoppingCart { Id = 1 };

            // Act
            var result = validator.CollectValidationResults(cart);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(5, result.Errors.Count);

            // Check filtered collections by severity
            var verboseErrors = result.GetErrorsBySeverity(ValidationSeverity.Verbose);
            var infoErrors = result.GetErrorsBySeverity(ValidationSeverity.Information);
            var warningErrors = result.GetErrorsBySeverity(ValidationSeverity.Warning);
            var errorErrors = result.GetErrorsBySeverity(ValidationSeverity.Error);
            var criticalErrors = result.GetErrorsBySeverity(ValidationSeverity.Critical);

            Assert.AreEqual(1, System.Linq.Enumerable.Count(verboseErrors));
            Assert.AreEqual(1, System.Linq.Enumerable.Count(infoErrors));
            Assert.AreEqual(1, System.Linq.Enumerable.Count(warningErrors));
            Assert.AreEqual(1, System.Linq.Enumerable.Count(errorErrors));
            Assert.AreEqual(1, System.Linq.Enumerable.Count(criticalErrors));

            Assert.AreEqual("Verbose level message", System.Linq.Enumerable.First(verboseErrors).Message);
            Assert.AreEqual("Information level message", System.Linq.Enumerable.First(infoErrors).Message);
            Assert.AreEqual("Warning level message", System.Linq.Enumerable.First(warningErrors).Message);
            Assert.AreEqual("Error level message", System.Linq.Enumerable.First(errorErrors).Message);
            Assert.AreEqual("Critical level message", System.Linq.Enumerable.First(criticalErrors).Message);
        }

        // Helper class for testing if context properties can be accessed
        private class ContextAwareTestRule : IdentifiableValidationRule<ShoppingCart>
        {
            public bool ContextAccessSuccessful { get; private set; }

            public ContextAwareTestRule(string id) : base(id) { }

            public override ValidationSeverity DefaultSeverity => ValidationSeverity.Error;

            public override void Validate(ShoppingCart input)
            {
                // This rule would throw if the context access fails
                var context = new ValidationContext();
                if (context.Properties.TryGetValue("CalculatedTotal", out var calculatedTotal))
                {
                    decimal total = (decimal)calculatedTotal;
                    ContextAccessSuccessful = Math.Abs(total - input.TotalAmount) <= 0.01m;
                }
                else
                {
                    ContextAccessSuccessful = false;
                }

                if (!ContextAccessSuccessful)
                {
                    throw new ArgumentException("Context property 'CalculatedTotal' was not accessible");
                }
            }
        }

        // Helper class for testing if rule results can be accessed
        private class RuleResultCheckingRule : IdentifiableValidationRule<ShoppingCart>
        {
            private readonly string[] _rulesToCheck;
            public bool AllRulesSucceeded { get; private set; }

            public RuleResultCheckingRule(string id, string[] rulesToCheck) : base(id)
            {
                _rulesToCheck = rulesToCheck;
            }

            public override ValidationSeverity DefaultSeverity => ValidationSeverity.Error;

            public override void Validate(ShoppingCart input)
            {
                var context = new ValidationContext();

                AllRulesSucceeded = true;
                foreach (var ruleId in _rulesToCheck)
                {
                    if (!context.RuleResults.TryGetValue(ruleId, out var result) || !result.Success)
                    {
                        AllRulesSucceeded = false;
                        throw new ArgumentException($"Rule {ruleId} did not succeed");
                    }
                }
            }
        }

        // Helper class for checking if a rule failure can be detected
        private class RuleFailureCheckingRule : IdentifiableValidationRule<ShoppingCart>
        {
            private readonly string _ruleToCheck;
            public bool FailureDetected { get; private set; }

            public RuleFailureCheckingRule(string id, string ruleToCheck) : base(id)
            {
                _ruleToCheck = ruleToCheck;
            }

            public override ValidationSeverity DefaultSeverity => ValidationSeverity.Error;

            public override void Validate(ShoppingCart input)
            {
                var context = new ValidationContext();

                if (context.RuleResults.TryGetValue(_ruleToCheck, out var result) && !result.Success)
                {
                    FailureDetected = true;
                }
                else
                {
                    FailureDetected = false;
                    throw new ArgumentException($"Failed to detect failure of rule {_ruleToCheck}");
                }
            }
        }
    }
}
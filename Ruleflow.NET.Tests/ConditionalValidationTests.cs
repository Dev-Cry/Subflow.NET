using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ruleflow.NET.Engine.Validation;
using Ruleflow.NET.Engine.Validation.Enums;
using System;

namespace Ruleflow.NET.Tests
{
    [TestClass]
    public class ConditionalValidationTests
    {
        private class Order
        {
            public int Id { get; set; }
            public decimal Amount { get; set; }
            public bool IsExpressShipping { get; set; }
            public OrderStatus Status { get; set; }
            public DateTime? SubmissionDate { get; set; }
            public DateTime? CompletionDate { get; set; }
            public string CustomerType { get; set; }
        }

        private enum OrderStatus
        {
            Draft,
            Submitted,
            Processing,
            Completed,
            Cancelled
        }

        [TestMethod]
        public void ConditionalRule_ThenBranchExecuted_WhenConditionIsTrue()
        {
            // Arrange
            var rule = RuleflowExtensions
                .CreateConditionalRule<Order>(o => o.Amount > 100)
                .Then(builder => builder
                    .WithAction(o => {
                        if (!o.IsExpressShipping)
                            throw new ArgumentException("Express shipping is required for orders over $100");
                    })
                    .WithMessage("Shipping validation failed")
                )
                .Build();

            var order = new Order
            {
                Id = 1,
                Amount = 150,
                IsExpressShipping = false
            };

            // Act & Assert
            var ex = Assert.ThrowsException<ArgumentException>(() => rule.Validate(order));
            Assert.AreEqual("Express shipping is required for orders over $100", ex.Message);
        }

        [TestMethod]
        public void ConditionalRule_ThenBranchNotExecuted_WhenConditionIsFalse()
        {
            // Arrange
            var rule = RuleflowExtensions
                .CreateConditionalRule<Order>(o => o.Amount > 100)
                .Then(builder => builder
                    .WithAction(o => {
                        if (!o.IsExpressShipping)
                            throw new ArgumentException("Express shipping is required for orders over $100");
                    })
                    .WithMessage("Shipping validation failed")
                )
                .Build();

            var order = new Order
            {
                Id = 1,
                Amount = 50,  // Below threshold, condition is false
                IsExpressShipping = false
            };

            // Act & Assert - No exception should be thrown
            rule.Validate(order);
        }

        [TestMethod]
        public void ConditionalRule_ElseBranchExecuted_WhenConditionIsFalse()
        {
            // Arrange
            var rule = RuleflowExtensions
                .CreateConditionalRule<Order>(o => o.Amount > 100)
                .Then(builder => builder
                    .WithAction(o => {
                        if (!o.IsExpressShipping)
                            throw new ArgumentException("Express shipping is required for orders over $100");
                    })
                )
                .Else(builder => builder
                    .WithAction(o => {
                        if (o.IsExpressShipping)
                            throw new ArgumentException("Express shipping is not available for orders under $100");
                    })
                )
                .Build();

            var order = new Order
            {
                Id = 1,
                Amount = 50,  // Below threshold, condition is false
                IsExpressShipping = true
            };

            // Act & Assert
            var ex = Assert.ThrowsException<ArgumentException>(() => rule.Validate(order));
            Assert.AreEqual("Express shipping is not available for orders under $100", ex.Message);
        }

        [TestMethod]
        public void ConditionalRule_ElseIfBranchExecuted_WhenItsConditionIsTrue()
        {
            // Arrange
            var rule = RuleflowExtensions
                .CreateConditionalRule<Order>(o => o.Amount > 500)
                .Then(builder => builder
                    .WithAction(o => {
                        if (!o.IsExpressShipping)
                            throw new ArgumentException("Express shipping is required for premium orders");
                    })
                )
                .ElseIf(o => o.Amount > 100)
                .Then(builder => builder
                    .WithAction(o => {
                        if (o.CustomerType != "Preferred")
                            throw new ArgumentException("Only preferred customers can place orders between $100-$500");
                    })
                )
                .Else(builder => builder
                    .WithAction(o => { /* No validation for small orders */ })
                )
                .Build();

            var order = new Order
            {
                Id = 1,
                Amount = 200,  // Between $100-$500, should hit the ElseIf branch
                IsExpressShipping = false,
                CustomerType = "Regular"  // Not a preferred customer
            };

            // Act & Assert
            var ex = Assert.ThrowsException<ArgumentException>(() => rule.Validate(order));
            Assert.AreEqual("Only preferred customers can place orders between $100-$500", ex.Message);
        }

        [TestMethod]
        public void ConditionalRule_MultipleElseIfBranches_CorrectOneExecuted()
        {
            // Arrange
            var rule = RuleflowExtensions
                .CreateConditionalRule<Order>(o => o.Amount > 1000)
                .Then(builder => builder
                    .WithAction(o => {
                        throw new ArgumentException("Validation for very large orders");
                    })
                )
                .ElseIf(o => o.Amount > 500)
                .Then(builder => builder
                    .WithAction(o => {
                        throw new ArgumentException("Validation for large orders");
                    })
                )
                .ElseIf(o => o.Amount > 100)
                .Then(builder => builder
                    .WithAction(o => {
                        throw new ArgumentException("Validation for medium orders");
                    })
                )
                .Else(builder => builder
                    .WithAction(o => {
                        throw new ArgumentException("Validation for small orders");
                    })
                )
                .Build();

            var order = new Order
            {
                Id = 1,
                Amount = 300  // Should hit the last ElseIf branch (> 100)
            };

            // Act & Assert
            var ex = Assert.ThrowsException<ArgumentException>(() => rule.Validate(order));
            Assert.AreEqual("Validation for medium orders", ex.Message);
        }

        [TestMethod]
        public void SwitchRule_ExecutesCorrectCase()
        {
            // Arrange
            var rule = RuleflowExtensions
                .CreateSwitchRule<Order, OrderStatus>(o => o.Status)
                .Case(OrderStatus.Draft, builder => builder
                    .WithAction(o => {
                        // No validation for drafts
                    })
                )
                .Case(OrderStatus.Submitted, builder => builder
                    .WithAction(o => {
                        if (o.SubmissionDate == null)
                            throw new ArgumentException("Submitted orders must have a submission date");
                    })
                )
                .Case(OrderStatus.Completed, builder => builder
                    .WithAction(o => {
                        if (o.CompletionDate == null)
                            throw new ArgumentException("Completed orders must have a completion date");
                    })
                )
                .Default(builder => builder
                    .WithAction(o => {
                        // Default validation
                    })
                )
                .Build();

            var order = new Order
            {
                Id = 1,
                Status = OrderStatus.Completed,
                CompletionDate = null  // Missing completion date
            };

            // Act & Assert
            var ex = Assert.ThrowsException<ArgumentException>(() => rule.Validate(order));
            Assert.AreEqual("Completed orders must have a completion date", ex.Message);
        }

        [TestMethod]
        public void SwitchRule_ExecutesDefaultCase_WhenNoMatchingCase()
        {
            // Arrange
            var rule = RuleflowExtensions
                .CreateSwitchRule<Order, OrderStatus>(o => o.Status)
                .Case(OrderStatus.Draft, builder => builder
                    .WithAction(o => {
                        // No validation for drafts
                    })
                )
                .Case(OrderStatus.Submitted, builder => builder
                    .WithAction(o => {
                        throw new ArgumentException("Submitted order validation");
                    })
                )
                .Default(builder => builder
                    .WithAction(o => {
                        throw new ArgumentException("Default validation for other statuses");
                    })
                )
                .Build();

            var order = new Order
            {
                Id = 1,
                Status = OrderStatus.Processing  // Not explicitly handled, should go to default
            };

            // Act & Assert
            var ex = Assert.ThrowsException<ArgumentException>(() => rule.Validate(order));
            Assert.AreEqual("Default validation for other statuses", ex.Message);
        }

        [TestMethod]
        public void SwitchRule_NoDefaultCase_DoesNotValidate_WhenNoMatchingCase()
        {
            // Arrange
            var rule = RuleflowExtensions
                .CreateSwitchRule<Order, OrderStatus>(o => o.Status)
                .Case(OrderStatus.Draft, builder => builder
                    .WithAction(o => {
                        throw new ArgumentException("Draft validation");
                    })
                )
                .Case(OrderStatus.Submitted, builder => builder
                    .WithAction(o => {
                        throw new ArgumentException("Submitted validation");
                    })
                )
                .Build();  // No default case defined

            var order = new Order
            {
                Id = 1,
                Status = OrderStatus.Processing  // Not handled by any case
            };

            // Act & Assert - No exception should be thrown
            rule.Validate(order);
        }

        [TestMethod]
        public void SwitchRule_WithDifferentValueTypes_String()
        {
            // Arrange
            var rule = RuleflowExtensions
                .CreateSwitchRule<Order, string>(o => o.CustomerType)
                .Case("Premium", builder => builder
                    .WithAction(o => {
                        throw new ArgumentException("Premium customer validation");
                    })
                )
                .Case("Regular", builder => builder
                    .WithAction(o => {
                        throw new ArgumentException("Regular customer validation");
                    })
                )
                .Default(builder => builder
                    .WithAction(o => {
                        throw new ArgumentException("Unknown customer type");
                    })
                )
                .Build();

            var order = new Order
            {
                Id = 1,
                CustomerType = "Regular"
            };

            // Act & Assert
            var ex = Assert.ThrowsException<ArgumentException>(() => rule.Validate(order));
            Assert.AreEqual("Regular customer validation", ex.Message);
        }
    }
}
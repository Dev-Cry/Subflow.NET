using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ruleflow.NET.Engine.Validation;
using Ruleflow.NET.Engine.Validation.Core.Validators;
using Ruleflow.NET.Engine.Validation.Enums;
using System;
using System.Collections.Generic;

namespace Ruleflow.NET.Tests
{
    [TestClass]
    public class ValidationExtensionsTests
    {
        private class Product
        {
            public string Name { get; set; }
            public decimal Price { get; set; }
            public string Category { get; set; }
            public int StockQuantity { get; set; }
        }

        [TestMethod]
        public void IsValid_WithValidInput_ReturnsTrue()
        {
            // Arrange
            var priceRule = RuleflowExtensions.CreateRule<Product>()
                .WithAction(p => {
                    if (p.Price <= 0)
                        throw new ArgumentException("Price must be positive");
                })
                .Build();

            var stockRule = RuleflowExtensions.CreateRule<Product>()
                .WithAction(p => {
                    if (p.StockQuantity < 0)
                        throw new ArgumentException("Stock quantity cannot be negative");
                })
                .Build();

            var validator = new Validator<Product>(new[] { priceRule, stockRule });

            var validProduct = new Product
            {
                Name = "Valid Product",
                Price = 10.99m,
                StockQuantity = 5
            };

            // Act
            bool isValid = validator.IsValid(validProduct);

            // Assert
            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void IsValid_WithInvalidInput_ReturnsFalse()
        {
            // Arrange
            var priceRule = RuleflowExtensions.CreateRule<Product>()
                .WithAction(p => {
                    if (p.Price <= 0)
                        throw new ArgumentException("Price must be positive");
                })
                .Build();

            var validator = new Validator<Product>(new[] { priceRule });

            var invalidProduct = new Product
            {
                Name = "Invalid Product",
                Price = 0,
                StockQuantity = 5
            };

            // Act
            bool isValid = validator.IsValid(invalidProduct);

            // Assert
            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void GetFirstError_WithValidInput_ReturnsNull()
        {
            // Arrange
            var priceRule = RuleflowExtensions.CreateRule<Product>()
                .WithAction(p => {
                    if (p.Price <= 0)
                        throw new ArgumentException("Price must be positive");
                })
                .Build();

            var validator = new Validator<Product>(new[] { priceRule });

            var validProduct = new Product
            {
                Name = "Valid Product",
                Price = 10.99m,
                StockQuantity = 5
            };

            // Act
            var error = validator.GetFirstError(validProduct);

            // Assert
            Assert.IsNull(error);
        }

        [TestMethod]
        public void GetFirstError_WithInvalidInput_ReturnsError()
        {
            // Arrange
            var priceRule = RuleflowExtensions.CreateRule<Product>()
                .WithAction(p => {
                    if (p.Price <= 0)
                        throw new ArgumentException("Price must be positive");
                })
                .Build();

            var validator = new Validator<Product>(new[] { priceRule });

            var invalidProduct = new Product
            {
                Name = "Invalid Product",
                Price = 0,
                StockQuantity = 5
            };

            // Act
            var error = validator.GetFirstError(invalidProduct);

            // Assert
            Assert.IsNotNull(error);
            Assert.AreEqual("Price must be positive", error.Message);
        }

        [TestMethod]
        public void ValidateAndExecute_WithValidInput_ExecutesSuccessAction()
        {
            // Arrange
            var priceRule = RuleflowExtensions.CreateRule<Product>()
                .WithAction(p => {
                    if (p.Price <= 0)
                        throw new ArgumentException("Price must be positive");
                })
                .Build();

            var validator = new Validator<Product>(new[] { priceRule });

            var validProduct = new Product
            {
                Name = "Valid Product",
                Price = 10.99m,
                StockQuantity = 5
            };

            bool successExecuted = false;
            bool failureExecuted = false;

            // Act
            validator.ValidateAndExecute(
                validProduct,
                () => successExecuted = true,
                errors => failureExecuted = true
            );

            // Assert
            Assert.IsTrue(successExecuted);
            Assert.IsFalse(failureExecuted);
        }

        [TestMethod]
        public void ValidateAndExecute_WithInvalidInput_ExecutesFailureAction()
        {
            // Arrange
            var priceRule = RuleflowExtensions.CreateRule<Product>()
                .WithAction(p => {
                    if (p.Price <= 0)
                        throw new ArgumentException("Price must be positive");
                })
                .Build();

            var validator = new Validator<Product>(new[] { priceRule });

            var invalidProduct = new Product
            {
                Name = "Invalid Product",
                Price = 0,
                StockQuantity = 5
            };

            bool successExecuted = false;
            bool failureExecuted = false;
            string errorMessage = null;

            // Act
            validator.ValidateAndExecute(
                invalidProduct,
                () => successExecuted = true,
                errors => {
                    failureExecuted = true;
                    if (errors.Count > 0)
                        errorMessage = errors[0].Message;
                }
            );

            // Assert
            Assert.IsFalse(successExecuted);
            Assert.IsTrue(failureExecuted);
            Assert.AreEqual("Price must be positive", errorMessage);
        }

        [TestMethod]
        public void ValidateAndProcess_WithValidInput_ProcessesInput()
        {
            // Arrange
            var priceRule = RuleflowExtensions.CreateRule<Product>()
                .WithAction(p => {
                    if (p.Price <= 0)
                        throw new ArgumentException("Price must be positive");
                })
                .Build();

            var validator = new Validator<Product>(new[] { priceRule });

            var validProduct = new Product
            {
                Name = "Valid Product",
                Price = 10.99m,
                StockQuantity = 5
            };

            bool processingExecuted = false;
            decimal processedPrice = 0;

            // Act
            var result = validator.ValidateAndProcess(
                validProduct,
                p => {
                    processingExecuted = true;
                    processedPrice = p.Price;
                }
            );

            // Assert
            Assert.IsTrue(result.IsValid);
            Assert.IsTrue(processingExecuted);
            Assert.AreEqual(10.99m, processedPrice);
        }

        [TestMethod]
        public void ValidateAndProcess_WithInvalidInput_DoesNotProcessInput()
        {
            // Arrange
            var priceRule = RuleflowExtensions.CreateRule<Product>()
                .WithAction(p => {
                    if (p.Price <= 0)
                        throw new ArgumentException("Price must be positive");
                })
                .Build();

            var validator = new Validator<Product>(new[] { priceRule });

            var invalidProduct = new Product
            {
                Name = "Invalid Product",
                Price = 0,
                StockQuantity = 5
            };

            bool processingExecuted = false;

            // Act
            var result = validator.ValidateAndProcess(
                invalidProduct,
                p => {
                    processingExecuted = true;
                }
            );

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsFalse(processingExecuted);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("Price must be positive", result.Errors[0].Message);
        }

        [TestMethod]
        public void Validate_ExtensionMethod_ReturnsValidationResult()
        {
            // Arrange
            var nameRule = RuleflowExtensions.CreateRule<Product>()
                .WithAction(p => {
                    if (string.IsNullOrEmpty(p.Name))
                        throw new ArgumentException("Name is required");
                })
                .Build();

            var priceRule = RuleflowExtensions.CreateRule<Product>()
                .WithAction(p => {
                    if (p.Price <= 0)
                        throw new ArgumentException("Price must be positive");
                })
                .Build();

            var rules = new[] { nameRule, priceRule };

            var product = new Product
            {
                Name = "Test Product",
                Price = 15.99m
            };

            // Act
            var result = product.Validate(rules);

            // Assert
            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(0, result.Errors.Count);
        }
    }
}
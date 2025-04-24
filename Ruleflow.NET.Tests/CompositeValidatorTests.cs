using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ruleflow.NET.Engine.Validation;
using Ruleflow.NET.Engine.Validation.Core.Validators;
using Ruleflow.NET.Engine.Validation.Enums;
using System;
using System.Collections.Generic;

namespace Ruleflow.NET.Tests
{
    [TestClass]
    public class CompositeValidatorTests
    {
        private class Customer
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public int Age { get; set; }
            public string PhoneNumber { get; set; }
        }

        [TestMethod]
        public void CompositeValidator_AllValidatorsSucceed_ReturnsSuccess()
        {
            // Arrange
            var nameValidator = new Validator<Customer>(new[] {
                RuleflowExtensions.CreateRule<Customer>()
                    .WithAction(c => {
                        if (string.IsNullOrEmpty(c.Name))
                            throw new ArgumentException("Name is required");
                    })
                    .Build()
            });

            var emailValidator = new Validator<Customer>(new[] {
                RuleflowExtensions.CreateRule<Customer>()
                    .WithAction(c => {
                        if (string.IsNullOrEmpty(c.Email))
                            throw new ArgumentException("Email is required");
                        if (!c.Email.Contains("@"))
                            throw new ArgumentException("Email must contain @");
                    })
                    .Build()
            });

            var compositeValidator = new CompositeValidator<Customer>(new[] { nameValidator, emailValidator });

            var validCustomer = new Customer
            {
                Name = "John Doe",
                Email = "john@example.com",
                Age = 30
            };

            // Act
            var result = compositeValidator.CollectValidationResults(validCustomer);

            // Assert
            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(0, result.Errors.Count);
        }

        [TestMethod]
        public void CompositeValidator_OneValidatorFails_ReturnsFailure()
        {
            // Arrange
            var nameValidator = new Validator<Customer>(new[] {
                RuleflowExtensions.CreateRule<Customer>()
                    .WithAction(c => {
                        if (string.IsNullOrEmpty(c.Name))
                            throw new ArgumentException("Name is required");
                    })
                    .Build()
            });

            var emailValidator = new Validator<Customer>(new[] {
                RuleflowExtensions.CreateRule<Customer>()
                    .WithAction(c => {
                        if (string.IsNullOrEmpty(c.Email))
                            throw new ArgumentException("Email is required");
                        if (!c.Email.Contains("@"))
                            throw new ArgumentException("Email must contain @");
                    })
                    .Build()
            });

            var compositeValidator = new CompositeValidator<Customer>(new[] { nameValidator, emailValidator });

            var invalidCustomer = new Customer
            {
                Name = "John Doe",
                Email = "invalid-email", // Missing @
                Age = 30
            };

            // Act
            var result = compositeValidator.CollectValidationResults(invalidCustomer);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("Email must contain @", result.Errors[0].Message);
        }

        [TestMethod]
        public void CompositeValidator_MultipleValidatorsFail_CollectsAllErrors()
        {
            // Arrange
            var nameValidator = new Validator<Customer>(new[] {
                RuleflowExtensions.CreateRule<Customer>()
                    .WithAction(c => {
                        if (string.IsNullOrEmpty(c.Name))
                            throw new ArgumentException("Name is required");
                    })
                    .Build()
            });

            var emailValidator = new Validator<Customer>(new[] {
                RuleflowExtensions.CreateRule<Customer>()
                    .WithAction(c => {
                        if (string.IsNullOrEmpty(c.Email))
                            throw new ArgumentException("Email is required");
                    })
                    .Build()
            });

            var ageValidator = new Validator<Customer>(new[] {
                RuleflowExtensions.CreateRule<Customer>()
                    .WithAction(c => {
                        if (c.Age < 18)
                            throw new ArgumentException("Customer must be at least 18 years old");
                    })
                    .Build()
            });

            var compositeValidator = new CompositeValidator<Customer>(
                new[] { nameValidator, emailValidator, ageValidator });

            var invalidCustomer = new Customer
            {
                Name = "", // Empty name
                Email = "", // Empty email
                Age = 16    // Under 18
            };

            // Act
            var result = compositeValidator.CollectValidationResults(invalidCustomer);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(3, result.Errors.Count);

            // Check for all expected error messages
            bool nameErrorFound = false;
            bool emailErrorFound = false;
            bool ageErrorFound = false;

            foreach (var error in result.Errors)
            {
                if (error.Message == "Name is required")
                    nameErrorFound = true;
                if (error.Message == "Email is required")
                    emailErrorFound = true;
                if (error.Message == "Customer must be at least 18 years old")
                    ageErrorFound = true;
            }

            Assert.IsTrue(nameErrorFound, "Name error not found");
            Assert.IsTrue(emailErrorFound, "Email error not found");
            Assert.IsTrue(ageErrorFound, "Age error not found");
        }

        [TestMethod]
        public void CompositeValidator_ValidateOrThrow_ThrowsForInvalidInput()
        {
            // Arrange
            var nameValidator = new Validator<Customer>(new[] {
                RuleflowExtensions.CreateRule<Customer>()
                    .WithAction(c => {
                        if (string.IsNullOrEmpty(c.Name))
                            throw new ArgumentException("Name is required");
                    })
                    .Build()
            });

            var emailValidator = new Validator<Customer>(new[] {
                RuleflowExtensions.CreateRule<Customer>()
                    .WithAction(c => {
                        if (string.IsNullOrEmpty(c.Email))
                            throw new ArgumentException("Email is required");
                    })
                    .Build()
            });

            var compositeValidator = new CompositeValidator<Customer>(new[] { nameValidator, emailValidator });

            var invalidCustomer = new Customer
            {
                Name = "John Doe",
                Email = "",  // Empty email
                Age = 30
            };

            // Act & Assert
            Assert.ThrowsException<AggregateException>(() => compositeValidator.ValidateOrThrow(invalidCustomer));
        }

        [TestMethod]
        public void CompositeValidator_DifferentSeverities_ReportsAllErrors()
        {
            // Arrange
            var requiredValidator = new Validator<Customer>(new[] {
                RuleflowExtensions.CreateRule<Customer>()
                    .WithAction(c => {
                        if (string.IsNullOrEmpty(c.Name))
                            throw new ArgumentException("Name is required");
                    })
                    .WithSeverity(ValidationSeverity.Error)
                    .Build()
            });

            var recommendedValidator = new Validator<Customer>(new[] {
                RuleflowExtensions.CreateRule<Customer>()
                    .WithAction(c => {
                        if (string.IsNullOrEmpty(c.PhoneNumber))
                            throw new ArgumentException("Phone number is recommended");
                    })
                    .WithSeverity(ValidationSeverity.Warning)
                    .Build()
            });

            var compositeValidator = new CompositeValidator<Customer>(
                new[] { requiredValidator, recommendedValidator });

            var customer = new Customer
            {
                Name = "John Doe",
                Email = "john@example.com",
                Age = 30,
                PhoneNumber = "" // Empty phone number (warning)
            };

            // Act
            var result = compositeValidator.CollectValidationResults(customer);

            // Assert
            Assert.IsTrue(result.IsValid); // Still valid because warning doesn't make it invalid
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("Phone number is recommended", result.Errors[0].Message);
            Assert.AreEqual(ValidationSeverity.Warning, result.Errors[0].Severity);
        }
    }
}
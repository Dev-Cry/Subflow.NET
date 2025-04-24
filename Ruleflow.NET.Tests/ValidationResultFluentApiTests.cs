using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ruleflow.NET.Engine.Validation;
using Ruleflow.NET.Engine.Validation.Core.Results;
using Ruleflow.NET.Engine.Validation.Core.Validators;
using Ruleflow.NET.Engine.Validation.Enums;
using System;
using System.Collections.Generic;

namespace Ruleflow.NET.Tests
{
    [TestClass]
    public class ValidationResultFluentApiTests
    {
        private class Invoice
        {
            public int Id { get; set; }
            public string InvoiceNumber { get; set; }
            public DateTime IssueDate { get; set; }
            public DateTime? DueDate { get; set; }
            public decimal Amount { get; set; }
            public string CustomerName { get; set; }
            public bool IsPaid { get; set; }
            public string Status { get; set; }
        }

        [TestMethod]
        public void ValidationResult_OnSuccess_ExecutesWithValidResult()
        {
            // Arrange
            bool onSuccessCalled = false;
            var result = new ValidationResult();  // Valid result by default

            // Act
            result.OnSuccess(() => onSuccessCalled = true);

            // Assert
            Assert.IsTrue(onSuccessCalled, "OnSuccess action should be called for valid result");
        }

        [TestMethod]
        public void ValidationResult_OnSuccess_DoesNotExecuteWithInvalidResult()
        {
            // Arrange
            bool onSuccessCalled = false;
            var result = new ValidationResult();
            result.AddError("Error message", ValidationSeverity.Error);  // Make result invalid

            // Act
            result.OnSuccess(() => onSuccessCalled = true);

            // Assert
            Assert.IsFalse(onSuccessCalled, "OnSuccess action should not be called for invalid result");
        }

        [TestMethod]
        public void ValidationResult_OnFailure_ExecutesWithInvalidResult()
        {
            // Arrange
            bool onFailureCalled = false;
            var capturedErrors = new List<ValidationError>();

            var result = new ValidationResult();
            result.AddError("Error message", ValidationSeverity.Error);  // Make result invalid

            // Act
            result.OnFailure(errors => {
                onFailureCalled = true;
                capturedErrors.AddRange(errors);
            });

            // Assert
            Assert.IsTrue(onFailureCalled, "OnFailure action should be called for invalid result");
            Assert.AreEqual(1, capturedErrors.Count);
            Assert.AreEqual("Error message", capturedErrors[0].Message);
        }

        [TestMethod]
        public void ValidationResult_OnFailure_DoesNotExecuteWithValidResult()
        {
            // Arrange
            bool onFailureCalled = false;
            var result = new ValidationResult();  // Valid result by default

            // Act
            result.OnFailure(errors => onFailureCalled = true);

            // Assert
            Assert.IsFalse(onFailureCalled, "OnFailure action should not be called for valid result");
        }

        [TestMethod]
        public void ValidationResult_MethodChaining_ExecutesCorrespondingAction()
        {
            // Arrange
            bool onSuccessCalled = false;
            bool onFailureCalled = false;

            // Valid result
            var validResult = new ValidationResult();

            // Invalid result
            var invalidResult = new ValidationResult();
            invalidResult.AddError("Error message", ValidationSeverity.Error);

            // Act
            validResult
                .OnSuccess(() => onSuccessCalled = true)
                .OnFailure(errors => onFailureCalled = true);

            bool validSuccessCalled = onSuccessCalled;
            bool validFailureCalled = onFailureCalled;

            // Reset flags
            onSuccessCalled = false;
            onFailureCalled = false;

            invalidResult
                .OnSuccess(() => onSuccessCalled = true)
                .OnFailure(errors => onFailureCalled = true);

            bool invalidSuccessCalled = onSuccessCalled;
            bool invalidFailureCalled = onFailureCalled;

            // Assert
            Assert.IsTrue(validSuccessCalled, "OnSuccess should be called for valid result");
            Assert.IsFalse(validFailureCalled, "OnFailure should not be called for valid result");

            Assert.IsFalse(invalidSuccessCalled, "OnSuccess should not be called for invalid result");
            Assert.IsTrue(invalidFailureCalled, "OnFailure should be called for invalid result");
        }

        [TestMethod]
        public void ValidationResult_OnSuccessWithInput_PassesInputToAction()
        {
            // Arrange
            var result = new ValidationResult();  // Valid result
            var invoice = new Invoice { Id = 1, InvoiceNumber = "INV-001" };
            Invoice capturedInvoice = null;

            // Act
            result.OnSuccess(invoice, captured => capturedInvoice = captured);

            // Assert
            Assert.IsNotNull(capturedInvoice, "Input should be passed to the action");
            Assert.AreEqual(invoice.Id, capturedInvoice.Id);
            Assert.AreEqual(invoice.InvoiceNumber, capturedInvoice.InvoiceNumber);
        }

        [TestMethod]
        public void ValidateAndProcess_WithValidInput_ProcessesInput()
        {
            // Arrange
            var invoiceRule = RuleflowExtensions.CreateRule<Invoice>()
                .WithAction(i => {
                    if (string.IsNullOrEmpty(i.InvoiceNumber))
                        throw new ArgumentException("Invoice number is required");
                })
                .Build();

            var validator = new Validator<Invoice>(new[] { invoiceRule });

            var invoice = new Invoice
            {
                Id = 1,
                InvoiceNumber = "INV-001",
                Amount = 100.0m
            };

            bool processingCalled = false;
            decimal capturedAmount = 0;

            // Act
            var result = validator.ValidateAndProcess(
                invoice,
                i => {
                    processingCalled = true;
                    capturedAmount = i.Amount;
                }
            );

            // Assert
            Assert.IsTrue(result.IsValid);
            Assert.IsTrue(processingCalled, "Processing action should be called for valid input");
            Assert.AreEqual(100.0m, capturedAmount);
        }

        [TestMethod]
        public void ValidateAndProcess_WithInvalidInput_DoesNotProcessInput()
        {
            // Arrange
            var invoiceRule = RuleflowExtensions.CreateRule<Invoice>()
                .WithAction(i => {
                    if (string.IsNullOrEmpty(i.InvoiceNumber))
                        throw new ArgumentException("Invoice number is required");
                })
                .Build();

            var validator = new Validator<Invoice>(new[] { invoiceRule });

            var invoice = new Invoice
            {
                Id = 1,
                InvoiceNumber = "", // Invalid - empty invoice number
                Amount = 100.0m
            };

            bool processingCalled = false;

            // Act
            var result = validator.ValidateAndProcess(
                invoice,
                i => {
                    processingCalled = true;
                }
            );

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsFalse(processingCalled, "Processing action should not be called for invalid input");
            Assert.AreEqual(1, result.Errors.Count);
            Assert.AreEqual("Invoice number is required", result.Errors[0].Message);
        }

        [TestMethod]
        public void ValidateAndExecute_WithValidInput_ExecutesSuccessAction()
        {
            // Arrange
            var invoiceRule = RuleflowExtensions.CreateRule<Invoice>()
                .WithAction(i => {
                    if (string.IsNullOrEmpty(i.InvoiceNumber))
                        throw new ArgumentException("Invoice number is required");
                })
                .Build();

            var validator = new Validator<Invoice>(new[] { invoiceRule });

            var invoice = new Invoice
            {
                Id = 1,
                InvoiceNumber = "INV-001"
            };

            bool successCalled = false;
            bool failureCalled = false;

            // Act
            validator.ValidateAndExecute(
                invoice,
                () => successCalled = true,
                errors => failureCalled = true
            );

            // Assert
            Assert.IsTrue(successCalled, "Success action should be called for valid input");
            Assert.IsFalse(failureCalled, "Failure action should not be called for valid input");
        }

        [TestMethod]
        public void ValidateAndExecute_WithInvalidInput_ExecutesFailureAction()
        {
            // Arrange
            var invoiceRule = RuleflowExtensions.CreateRule<Invoice>()
                .WithAction(i => {
                    if (string.IsNullOrEmpty(i.InvoiceNumber))
                        throw new ArgumentException("Invoice number is required");
                })
                .Build();

            var validator = new Validator<Invoice>(new[] { invoiceRule });

            var invoice = new Invoice
            {
                Id = 1,
                InvoiceNumber = ""  // Invalid - empty invoice number
            };

            bool successCalled = false;
            bool failureCalled = false;
            List<ValidationError> capturedErrors = new List<ValidationError>();

            // Act
            validator.ValidateAndExecute(
                invoice,
                () => successCalled = true,
                errors => {
                    failureCalled = true;
                    capturedErrors.AddRange(errors);
                }
            );

            // Assert
            Assert.IsFalse(successCalled, "Success action should not be called for invalid input");
            Assert.IsTrue(failureCalled, "Failure action should be called for invalid input");
            Assert.AreEqual(1, capturedErrors.Count);
            Assert.AreEqual("Invoice number is required", capturedErrors[0].Message);
        }

        [TestMethod]
        public void ValidationResult_OnSuccessAndOnFailure_ReturnsSameInstance_ForChaining()
        {
            // Arrange
            var result = new ValidationResult();

            // Act
            var returnedResult1 = result.OnSuccess(() => { });
            var returnedResult2 = result.OnFailure(errors => { });

            // Assert
            Assert.AreSame(result, returnedResult1, "OnSuccess should return the same ValidationResult instance");
            Assert.AreSame(result, returnedResult2, "OnFailure should return the same ValidationResult instance");
        }

        [TestMethod]
        public void ValidationResult_IsValid_ReturnsTrueForWarningsOnly()
        {
            // Arrange
            var result = new ValidationResult();

            // Add warning (which shouldn't make the result invalid)
            result.AddError("Warning message", ValidationSeverity.Warning);

            // Assert
            Assert.IsTrue(result.IsValid, "Result with warnings only should be considered valid");
            Assert.AreEqual(1, result.Errors.Count);
        }

        [TestMethod]
        public void ValidationResult_IsValid_ReturnsFalseForErrors()
        {
            // Arrange
            var result = new ValidationResult();

            // Add warning and error
            result.AddError("Warning message", ValidationSeverity.Warning);
            result.AddError("Error message", ValidationSeverity.Error);

            // Assert
            Assert.IsFalse(result.IsValid, "Result with errors should be considered invalid");
            Assert.AreEqual(2, result.Errors.Count);
        }

        [TestMethod]
        public void ValidationResult_HasCriticalErrors_OnlyChecksForCriticalSeverity()
        {
            // Arrange
            var result = new ValidationResult();

            // Add various error types
            result.AddError("Debug message", ValidationSeverity.Debug);
            result.AddError("Info message", ValidationSeverity.Information);
            result.AddError("Warning message", ValidationSeverity.Warning);
            result.AddError("Error message", ValidationSeverity.Error);

            // Assert - No critical errors yet
            Assert.IsFalse(result.HasCriticalErrors, "Result without critical errors should report HasCriticalErrors=false");
            Assert.IsFalse(result.IsValid, "Result with errors should be invalid");

            // Add a critical error
            result.AddError("Critical message", ValidationSeverity.Critical);

            // Assert - Now has critical errors
            Assert.IsTrue(result.HasCriticalErrors, "Result with critical errors should report HasCriticalErrors=true");
            Assert.IsFalse(result.IsValid, "Result with critical errors should be invalid");
        }

        [TestMethod]
        public void ThrowIfInvalid_WithValidResult_DoesNothing()
        {
            // Arrange
            var result = new ValidationResult();

            // Act & Assert - No exception should be thrown
            result.ThrowIfInvalid();
        }

        [TestMethod]
        public void ThrowIfInvalid_WithInvalidResult_ThrowsAggregateException()
        {
            // Arrange
            var result = new ValidationResult();
            result.AddError("Error 1", ValidationSeverity.Error);
            result.AddError("Error 2", ValidationSeverity.Error);

            // Act & Assert
            var ex = Assert.ThrowsException<AggregateException>(() => result.ThrowIfInvalid());
            Assert.AreEqual(2, ex.InnerExceptions.Count);
        }

        [TestMethod]
        public void ThrowIfInvalid_WithCriticalErrors_ThrowsWithCriticalMessage()
        {
            // Arrange
            var result = new ValidationResult();
            result.AddError("Regular error", ValidationSeverity.Error);
            result.AddError("Critical error", ValidationSeverity.Critical);

            // Act & Assert
            var ex = Assert.ThrowsException<AggregateException>(() => result.ThrowIfInvalid());
            Assert.IsTrue(ex.Message.Contains("kritickými"), "Exception message should mention critical errors");
        }

        [TestMethod]
        public void ComplexFluent_ValidationProcessing_CompletePipeline()
        {
            // Arrange
            // Create validation rules
            var invoiceNumberRule = RuleflowExtensions.CreateRule<Invoice>()
                .WithAction(i => {
                    if (string.IsNullOrEmpty(i.InvoiceNumber))
                        throw new ArgumentException("Invoice number is required");
                })
                .Build();

            var amountRule = RuleflowExtensions.CreateRule<Invoice>()
                .WithAction(i => {
                    if (i.Amount <= 0)
                        throw new ArgumentException("Amount must be positive");
                })
                .Build();

            var dueDateRule = RuleflowExtensions.CreateRule<Invoice>()
                .WithAction(i => {
                    if (i.DueDate == null)
                        throw new ArgumentException("Due date is required");
                    if (i.DueDate < i.IssueDate)
                        throw new ArgumentException("Due date cannot be before issue date");
                })
                .Build();

            var validator = new Validator<Invoice>(new[] {
                invoiceNumberRule,
                amountRule,
                dueDateRule
            });

            // Valid invoice
            var validInvoice = new Invoice
            {
                Id = 1,
                InvoiceNumber = "INV-001",
                Amount = 150.0m,
                IssueDate = new DateTime(2025, 1, 1),
                DueDate = new DateTime(2025, 1, 31),
                CustomerName = "Test Customer",
                Status = "Draft"
            };

            // Invalid invoice
            var invalidInvoice = new Invoice
            {
                Id = 2,
                InvoiceNumber = "INV-002",
                Amount = 0, // Invalid
                IssueDate = new DateTime(2025, 2, 1),
                DueDate = new DateTime(2025, 1, 15), // Invalid - before issue date
                CustomerName = "Test Customer",
                Status = "Draft"
            };

            // Tracking for flow execution
            string processedStatus = null;
            List<string> capturedErrors = new List<string>();

            // Act - Valid invoice processing
            validator.CollectValidationResults(validInvoice)
                .OnSuccess(invoice => {
                    invoice.Status = "Processed";
                    processedStatus = invoice.Status;
                })
                .OnFailure(errors => {
                    foreach (var error in errors)
                    {
                        capturedErrors.Add(error.Message);
                    }
                });

            // Assert - Valid invoice flow
            Assert.AreEqual("Processed", processedStatus);
            Assert.AreEqual(0, capturedErrors.Count);

            // Reset tracking
            processedStatus = null;
            capturedErrors.Clear();

            // Act - Invalid invoice processing
            validator.CollectValidationResults(invalidInvoice)
                .OnSuccess(invoice => {
                    invoice.Status = "Processed";
                    processedStatus = invoice.Status;
                })
                .OnFailure(errors => {
                    foreach (var error in errors)
                    {
                        capturedErrors.Add(error.Message);
                    }
                    invalidInvoice.Status = "Failed Validation";
                });

            // Assert - Invalid invoice flow
            Assert.IsNull(processedStatus);
            Assert.AreEqual(2, capturedErrors.Count);
            Assert.IsTrue(capturedErrors.Contains("Amount must be positive"));
            Assert.IsTrue(capturedErrors.Contains("Due date cannot be before issue date"));
            Assert.AreEqual("Failed Validation", invalidInvoice.Status);
        }
    }
}
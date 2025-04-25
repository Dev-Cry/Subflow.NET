using Ruleflow.NET.Engine.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ruleflow.NET.Engine.Models.ValidationResults
{
    /// <summary>
    /// Represents the result of a validation operation.
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// Gets a value indicating whether the validation succeeded.
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        /// Gets the rule that produced this result.
        /// </summary>
        public object Rule { get; }

        /// <summary>
        /// Gets the error message, if validation failed.
        /// </summary>
        public string ErrorMessage { get; }

        /// <summary>
        /// Gets the severity of the validation result.
        /// </summary>
        public RuleSeverity Severity { get; }

        /// <summary>
        /// Gets the exception that caused the validation failure, if any.
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// Gets any additional validation details.
        /// </summary>
        public IDictionary<string, object> Details { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationResult"/> class.
        /// </summary>
        /// <param name="isValid">Whether the validation succeeded.</param>
        /// <param name="rule">The rule that produced this result.</param>
        /// <param name="errorMessage">The error message, if validation failed.</param>
        /// <param name="severity">The severity of the validation result.</param>
        /// <param name="exception">The exception that caused the validation failure, if any.</param>
        /// <param name="details">Any additional validation details.</param>
        public ValidationResult(
            bool isValid,
            object rule,
            string errorMessage = null,
            RuleSeverity severity = RuleSeverity.Error,
            Exception exception = null,
            IDictionary<string, object> details = null)
        {
            IsValid = isValid;
            Rule = rule ?? throw new ArgumentNullException(nameof(rule));
            ErrorMessage = isValid ? null : errorMessage ?? "Validation failed";
            Severity = severity;
            Exception = exception;
            Details = details ?? new Dictionary<string, object>();
        }

        /// <summary>
        /// Creates a successful validation result.
        /// </summary>
        /// <param name="rule">The rule that produced this result.</param>
        /// <param name="details">Optional additional details about the successful validation.</param>
        /// <returns>A successful validation result.</returns>
        public static ValidationResult Success(object rule, IDictionary<string, object> details = null)
        {
            return new ValidationResult(true, rule, null, RuleSeverity.Information, null, details);
        }

        /// <summary>
        /// Creates a failed validation result.
        /// </summary>
        /// <param name="rule">The rule that produced this result.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="exception">The exception that caused the validation failure, if any.</param>
        /// <param name="severity">The severity of the failure.</param>
        /// <param name="details">Optional additional details about the validation failure.</param>
        /// <returns>A failed validation result.</returns>
        public static ValidationResult Failure(
            object rule,
            string errorMessage,
            Exception exception = null,
            RuleSeverity severity = RuleSeverity.Error,
            IDictionary<string, object> details = null)
        {
            return new ValidationResult(false, rule, errorMessage, severity, exception, details);
        }

        /// <summary>
        /// Returns a string that represents the current validation result.
        /// </summary>
        /// <returns>A string that represents the current validation result.</returns>
        public override string ToString()
        {
            if (IsValid)
            {
                return $"Success: Rule '{Rule}' passed validation.";
            }
            else
            {
                return $"{Severity}: Rule '{Rule}' failed validation. {ErrorMessage}";
            }
        }
    }
}
using System;

namespace Ruleflow.NET.Engine.Models.ValidationResults
{
    /// <summary>
    /// Represents an exception that is thrown when validation fails.
    /// </summary>
    public class ValidationException : Exception
    {
        /// <summary>
        /// Gets the validation report that contains the details of the validation failures.
        /// </summary>
        public ValidationReport ValidationReport { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="validationReport">The validation report that contains the details of the validation failures.</param>
        public ValidationException(string message, ValidationReport validationReport)
            : base(message)
        {
            ValidationReport = validationReport ?? throw new ArgumentNullException(nameof(validationReport));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="validationReport">The validation report that contains the details of the validation failures.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public ValidationException(string message, ValidationReport validationReport, Exception innerException)
            : base(message, innerException)
        {
            ValidationReport = validationReport ?? throw new ArgumentNullException(nameof(validationReport));
        }

        /// <summary>
        /// Returns a string that represents the current exception.
        /// </summary>
        /// <returns>A string representation of the current exception.</returns>
        public override string ToString()
        {
            return $"{base.ToString()}\n\nValidation Details:\n{ValidationReport.GetDetailedReport()}";
        }
    }
}
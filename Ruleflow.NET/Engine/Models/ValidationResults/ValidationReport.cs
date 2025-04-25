using Ruleflow.NET.Engine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruleflow.NET.Engine.Models.ValidationResults
{
    /// <summary>
    /// Represents a comprehensive report of multiple validation results.
    /// </summary>
    public class ValidationReport
    {
        private readonly List<ValidationResult> _results = new();

        /// <summary>
        /// Gets all validation results.
        /// </summary>
        public IReadOnlyList<ValidationResult> Results => _results.AsReadOnly();

        /// <summary>
        /// Gets the failed validation results (where IsValid is false).
        /// </summary>
        public IEnumerable<ValidationResult> Failures => _results.Where(r => !r.IsValid);

        /// <summary>
        /// Gets the successful validation results (where IsValid is true).
        /// </summary>
        public IEnumerable<ValidationResult> Successes => _results.Where(r => r.IsValid);

        /// <summary>
        /// Gets a value indicating whether all validations passed.
        /// </summary>
        public bool IsValid => !_results.Any(r => !r.IsValid);

        /// <summary>
        /// Gets a value indicating whether the validation report contains any critical failures.
        /// </summary>
        public bool HasCriticalFailures => _results.Any(r => !r.IsValid && r.Severity == RuleSeverity.Critical);

        /// <summary>
        /// Gets the error messages from all failed validations.
        /// </summary>
        public IEnumerable<string> ErrorMessages => Failures.Select(f => f.ErrorMessage);

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationReport"/> class.
        /// </summary>
        public ValidationReport()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationReport"/> class with initial results.
        /// </summary>
        /// <param name="results">The initial validation results.</param>
        public ValidationReport(IEnumerable<ValidationResult> results)
        {
            if (results != null)
            {
                _results.AddRange(results);
            }
        }

        /// <summary>
        /// Adds a validation result to the report.
        /// </summary>
        /// <param name="result">The validation result to add.</param>
        /// <returns>This validation report for method chaining.</returns>
        public ValidationReport AddResult(ValidationResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            _results.Add(result);
            return this;
        }

        /// <summary>
        /// Adds multiple validation results to the report.
        /// </summary>
        /// <param name="results">The validation results to add.</param>
        /// <returns>This validation report for method chaining.</returns>
        public ValidationReport AddResults(IEnumerable<ValidationResult> results)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            _results.AddRange(results);
            return this;
        }

        /// <summary>
        /// Gets validation failures of a specific severity.
        /// </summary>
        /// <param name="severity">The severity level to filter by.</param>
        /// <returns>Validation failures with the specified severity.</returns>
        public IEnumerable<ValidationResult> GetFailuresBySeverity(RuleSeverity severity)
        {
            return Failures.Where(f => f.Severity == severity);
        }

        /// <summary>
        /// Throws an exception if the validation report contains failures.
        /// </summary>
        /// <param name="exceptionFactory">A factory function that creates the exception to throw.</param>
        /// <exception cref="Exception">Thrown when the validation report contains failures.</exception>
        public void ThrowIfInvalid(Func<ValidationReport, Exception> exceptionFactory = null)
        {
            if (!IsValid)
            {
                if (exceptionFactory != null)
                {
                    throw exceptionFactory(this);
                }
                else
                {
                    throw new ValidationException("Validation failed.", this);
                }
            }
        }

        /// <summary>
        /// Creates a detailed string representation of the validation report.
        /// </summary>
        /// <returns>A string containing details of all validation results.</returns>
        public string GetDetailedReport()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Validation Report: {(IsValid ? "VALID" : "INVALID")}");
            sb.AppendLine($"Total Results: {_results.Count}");
            sb.AppendLine($"Successes: {Successes.Count()}, Failures: {Failures.Count()}");

            if (!IsValid)
            {
                sb.AppendLine("\nFailures:");
                foreach (var failure in Failures)
                {
                    sb.AppendLine($"- {failure.Severity}: {failure.ErrorMessage} (Rule: {failure.Rule})");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Returns a string that represents the current validation report.
        /// </summary>
        /// <returns>A string that represents the current validation report.</returns>
        public override string ToString()
        {
            if (IsValid)
            {
                return $"Valid ({_results.Count} rules passed)";
            }
            else
            {
                return $"Invalid ({Failures.Count()} failures out of {_results.Count} rules)";
            }
        }
    }
}
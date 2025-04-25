using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.CrossPlatEngine.Adapter;
using Ruleflow.NET.Engine.Models;
using Ruleflow.NET.Engine.Models.Context;
using Ruleflow.NET.Engine.Models.ValidationResults;

namespace Ruleflow.NET.Models
{
    /// <summary>
    /// Represents a basic rule in the Ruleflow.NET framework.
    /// A rule is a fundamental unit that can validate a specific input against defined criteria.
    /// </summary>
    /// <typeparam name="TInput">The type of input data that this rule validates.</typeparam>
    public class Rule<TInput>
    {
        /// <summary>
        /// Gets the unique identifier for this rule.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the descriptive name of this rule.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the detailed description of what this rule validates.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the error message to display when the rule validation fails.
        /// </summary>
        public string ErrorMessage { get; }

        /// <summary>
        /// Gets the severity level of this rule.
        /// </summary>
        public RuleSeverity Severity { get; }

        /// <summary>
        /// Gets the execution priority of this rule. Higher priority rules are executed first.
        /// </summary>
        public int Priority { get; }

        /// <summary>
        /// Gets the delegate that performs the actual validation logic.
        /// </summary>
        protected readonly Func<TInput, RuleContext, ValidationResult> ValidationFunction;

        /// <summary>
        /// Initializes a new instance of the <see cref="Rule{TInput}"/> class.
        /// </summary>
        /// <param name="id">The unique identifier for this rule.</param>
        /// <param name="name">The descriptive name of this rule.</param>
        /// <param name="validationFunction">The function that performs the validation.</param>
        /// <param name="errorMessage">The error message to display when validation fails.</param>
        /// <param name="severity">The severity level of this rule.</param>
        /// <param name="description">The detailed description of the rule.</param>
        /// <param name="priority">The execution priority of this rule.</param>
        public Rule(
        string id,
        string name,
            Func<TInput, RuleContext, ValidationResult> validationFunction,
            string errorMessage = "Validation failed",
            RuleSeverity severity = RuleSeverity.Error,
            string description = "",
            int priority = 0)
        {
            Id = string.IsNullOrEmpty(id) ? Guid.NewGuid().ToString() : id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            ValidationFunction = validationFunction ?? throw new ArgumentNullException(nameof(validationFunction));
            ErrorMessage = errorMessage ?? throw new ArgumentNullException(nameof(errorMessage));
            Severity = severity;
            Description = description ?? string.Empty;
            Priority = priority;
        }

        /// <summary>
        /// Validates the specified input against this rule.
        /// </summary>
        /// <param name="input">The input to validate.</param>
        /// <param name="context">The context in which validation occurs.</param>
        /// <returns>A <see cref="ValidationResult"/> indicating whether validation succeeded or failed.</returns>
        public virtual ValidationResult Validate(TInput input, RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // Check for cancellation request
            context.CancellationToken.ThrowIfCancellationRequested();

            try
            {
                // Execute the validation function
                var result = ValidationFunction(input, context);

                // Record the validation result in the context
                context.RecordRuleResult(Id, result);

                return result;
            }
            catch (OperationCanceledException)
            {
                // Re-throw cancellation exceptions
                throw;
            }
            catch (Exception ex) when (!(ex is ArgumentException))
            {
                // For other exceptions, create a failed validation result
                var result = ValidationResult.Failure(this, ErrorMessage, ex);

                // Record the validation result in the context
                context.RecordRuleResult(Id, result);

                return result;
            }
        }

        /// <summary>
        /// Asynchronously validates the specified input against this rule.
        /// </summary>
        /// <param name="input">The input to validate.</param>
        /// <param name="context">The context in which validation occurs.</param>
        /// <returns>A task that represents the asynchronous validation operation.</returns>
        public virtual async Task<ValidationResult> ValidateAsync(TInput input, RuleContext context)
        {
            // For basic rules, we just run the synchronous validation on a background thread
            return await Task.Run(() => Validate(input, context), context.CancellationToken);
        }

        /// <summary>
        /// Returns a string that represents the current rule.
        /// </summary>
        /// <returns>A string that represents the current rule.</returns>
        public override string ToString()
        {
            return $"Rule '{Name}' (ID: {Id}, Severity: {Severity})";
        }
    }
}
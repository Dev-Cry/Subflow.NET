using System;
using System.Threading.Tasks;
using Ruleflow.NET.Engine.Models.Context;
using Ruleflow.NET.Engine.Models;
using Ruleflow.NET.Engine.Models.Context;
using Ruleflow.NET.Engine.Models.ValidationResults;
using Ruleflow.NET.Models;

namespace Ruleflow.NET.Engine.Models.Rules
{
    /// <summary>
    /// Represents a rule that is executed only when a specific condition is met.
    /// </summary>
    /// <typeparam name="TInput">The type of input data that this rule validates.</typeparam>
    public class ConditionalRule<TInput> : Rule<TInput>
    {
        /// <summary>
        /// Gets the condition that determines whether the rule should be executed.
        /// </summary>
        protected readonly Func<TInput, RuleContext, bool> Condition;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionalRule{TInput}"/> class.
        /// </summary>
        /// <param name="id">The unique identifier for this rule.</param>
        /// <param name="name">The descriptive name of this rule.</param>
        /// <param name="validationFunction">The function that performs the validation.</param>
        /// <param name="condition">The condition that determines whether the rule should be executed.</param>
        /// <param name="errorMessage">The error message to display when validation fails.</param>
        /// <param name="severity">The severity level of this rule.</param>
        /// <param name="description">The detailed description of the rule.</param>
        /// <param name="priority">The execution priority of this rule.</param>
        public ConditionalRule(
            string id,
            string name,
            Func<TInput, RuleContext, ValidationResult> validationFunction,
            Func<TInput, RuleContext, bool> condition,
            string errorMessage = "Validation failed",
            RuleSeverity severity = RuleSeverity.Error,
            string description = "",
            int priority = 0)
            : base(id, name, validationFunction, errorMessage, severity, description, priority)
        {
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
        }

        /// <summary>
        /// Validates the specified input against this rule if the condition is met.
        /// </summary>
        /// <param name="input">The input to validate.</param>
        /// <param name="context">The context in which validation occurs.</param>
        /// <returns>A <see cref="ValidationResult"/> indicating whether validation succeeded or failed.</returns>
        public override ValidationResult Validate(TInput input, RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // Check for cancellation request
            context.CancellationToken.ThrowIfCancellationRequested();

            // Check if the condition is met
            if (!Condition(input, context))
            {
                // Condition not met, skip validation and return success
                var result = ValidationResult.Success(this);
                context.RecordRuleResult(Id, result);
                return result;
            }

            // Condition met, proceed with normal validation
            return base.Validate(input, context);
        }

        /// <summary>
        /// Asynchronously validates the specified input against this rule if the condition is met.
        /// </summary>
        /// <param name="input">The input to validate.</param>
        /// <param name="context">The context in which validation occurs.</param>
        /// <returns>A task that represents the asynchronous validation operation.</returns>
        public override async Task<ValidationResult> ValidateAsync(TInput input, RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // Check for cancellation request
            context.CancellationToken.ThrowIfCancellationRequested();

            // Check if the condition is met
            if (!Condition(input, context))
            {
                // Condition not met, skip validation and return success
                var result = ValidationResult.Success(this);
                context.RecordRuleResult(Id, result);
                return result;
            }

            // Condition met, proceed with normal validation
            return await base.ValidateAsync(input, context);
        }
    }
}
///
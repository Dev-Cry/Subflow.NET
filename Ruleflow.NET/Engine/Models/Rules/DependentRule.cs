using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ruleflow.NET.Engine.Models.Context;
using Ruleflow.NET.Engine.Models;
using Ruleflow.NET.Engine.Models.ValidationResults;
using Ruleflow.NET.Models;

namespace Ruleflow.NET.Engine.Models.Rules
{
    /// <summary>
    /// Represents the type of dependency between rules.
    /// </summary>
    public enum DependencyType
    {
        /// <summary>
        /// The rule should execute only if all dependencies succeeded.
        /// </summary>
        RequiresAllSuccess,

        /// <summary>
        /// The rule should execute only if any dependency succeeded.
        /// </summary>
        RequiresAnySuccess,

        /// <summary>
        /// The rule should execute only if all dependencies failed.
        /// </summary>
        RequiresAllFailure,

        /// <summary>
        /// The rule should execute only if any dependency failed.
        /// </summary>
        RequiresAnyFailure
    }

    /// <summary>
    /// Represents a rule that depends on the results of other rules.
    /// </summary>
    /// <typeparam name="TInput">The type of input data that this rule validates.</typeparam>
    public class DependentRule<TInput> : Rule<TInput>
    {
        /// <summary>
        /// Gets the IDs of the rules that this rule depends on.
        /// </summary>
        public IReadOnlyList<string> DependsOn { get; }

        /// <summary>
        /// Gets the type of dependency relationship.
        /// </summary>
        public DependencyType DependencyType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependentRule{TInput}"/> class.
        /// </summary>
        /// <param name="id">The unique identifier for this rule.</param>
        /// <param name="name">The descriptive name of this rule.</param>
        /// <param name="validationFunction">The function that performs the validation.</param>
        /// <param name="dependsOn">The IDs of the rules that this rule depends on.</param>
        /// <param name="dependencyType">The type of dependency relationship.</param>
        /// <param name="errorMessage">The error message to display when validation fails.</param>
        /// <param name="severity">The severity level of this rule.</param>
        /// <param name="description">The detailed description of the rule.</param>
        /// <param name="priority">The execution priority of this rule.</param>
        public DependentRule(
            string id,
            string name,
            Func<TInput, RuleContext, ValidationResult> validationFunction,
            IEnumerable<string> dependsOn,
            DependencyType dependencyType = DependencyType.RequiresAllSuccess,
            string errorMessage = "Validation failed",
            RuleSeverity severity = RuleSeverity.Error,
            string description = "",
            int priority = 0)
            : base(id, name, validationFunction, errorMessage, severity, description, priority)
        {
            if (dependsOn == null)
            {
                throw new ArgumentNullException(nameof(dependsOn));
            }

            var dependencyList = dependsOn.ToList();
            if (dependencyList.Count == 0)
            {
                throw new ArgumentException("At least one dependency must be specified.", nameof(dependsOn));
            }

            DependsOn = dependencyList;
            DependencyType = dependencyType;
        }

        /// <summary>
        /// Determines whether this rule should be executed based on the results of its dependencies.
        /// </summary>
        /// <param name="context">The context containing previous rule results.</param>
        /// <returns>True if the rule should be executed, false otherwise.</returns>
        protected virtual bool ShouldExecute(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return DependencyType switch
            {
                DependencyType.RequiresAllSuccess => context.AllRulesSucceeded(DependsOn),
                DependencyType.RequiresAnySuccess => context.AnyRuleSucceeded(DependsOn),
                DependencyType.RequiresAllFailure => context.AllRulesFailed(DependsOn),
                DependencyType.RequiresAnyFailure => context.AnyRuleFailed(DependsOn),
                _ => throw new NotSupportedException($"Dependency type '{DependencyType}' is not supported.")
            };
        }

        /// <summary>
        /// Validates the specified input against this rule if the dependency conditions are met.
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

            // Check if the dependencies allow this rule to execute
            if (!ShouldExecute(context))
            {
                // Dependencies not satisfied, skip validation and record result
                var result = ValidationResult.Success(this, new Dictionary<string, object>
                {
                    { "SkipReason", "Dependencies not satisfied" },
                    { "DependencyType", DependencyType },
                    { "DependsOn", DependsOn }
                });

                context.RecordRuleResult(Id, result);
                return result;
            }

            // Dependencies satisfied, proceed with normal validation
            return base.Validate(input, context);
        }

        /// <summary>
        /// Asynchronously validates the specified input against this rule if the dependency conditions are met.
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

            // Check if the dependencies allow this rule to execute
            if (!ShouldExecute(context))
            {
                // Dependencies not satisfied, skip validation and record result
                var result = ValidationResult.Success(this, new Dictionary<string, object>
                {
                    { "SkipReason", "Dependencies not satisfied" },
                    { "DependencyType", DependencyType },
                    { "DependsOn", DependsOn }
                });

                context.RecordRuleResult(Id, result);
                return result;
            }

            // Dependencies satisfied, proceed with normal validation
            return await base.ValidateAsync(input, context);
        }
    }
}
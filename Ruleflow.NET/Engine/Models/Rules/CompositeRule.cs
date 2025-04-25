using Ruleflow.NET.Engine.Models.Context;
using Ruleflow.NET.Engine.Models;

using Ruleflow.NET.Models;
using Ruleflow.NET.Engine.Models.ValidationResults;

namespace Ruleflow.NET.Engine.Models.Rules
{

    /// <summary>
    /// Represents a rule that is composed of multiple child rules.
    /// </summary>
    /// <typeparam name="TInput">The type of input data that this rule validates.</typeparam>
    public class CompositeRule<TInput> : Rule<TInput>
    {
        private readonly List<Rule<TInput>> _childRules = new();

        /// <summary>
        /// Gets the child rules that compose this rule.
        /// </summary>
        public IReadOnlyList<Rule<TInput>> ChildRules => _childRules.AsReadOnly();

        /// <summary>
        /// Gets the composition mode that determines how child rule results are combined.
        /// </summary>
        public CompositionMode CompositionMode { get; }

        /// <summary>
        /// Gets the threshold value used when CompositionMode is Threshold.
        /// </summary>
        public int? PassingThreshold { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeRule{TInput}"/> class.
        /// </summary>
        /// <param name="id">The unique identifier for this rule.</param>
        /// <param name="name">The descriptive name of this rule.</param>
        /// <param name="childRules">The child rules that compose this rule.</param>
        /// <param name="compositionMode">The composition mode that determines how child rule results are combined.</param>
        /// <param name="passingThreshold">The threshold value used when CompositionMode is Threshold.</param>
        /// <param name="errorMessage">The error message to display when validation fails.</param>
        /// <param name="severity">The severity level of this rule.</param>
        /// <param name="description">The detailed description of the rule.</param>
        /// <param name="priority">The execution priority of this rule.</param>
        public CompositeRule(
            string id,
            string name,
            IEnumerable<Rule<TInput>> childRules,
            CompositionMode compositionMode = CompositionMode.All,
            int? passingThreshold = null,
            string errorMessage = "Validation failed",
            RuleSeverity severity = RuleSeverity.Error,
            string description = "",
            int priority = 0)
            : base(id, name, null, errorMessage, severity, description, priority)
        {
            if (childRules == null)
            {
                throw new ArgumentNullException(nameof(childRules));
            }

            var rulesList = childRules.ToList();
            if (rulesList.Count == 0)
            {
                throw new ArgumentException("At least one child rule must be specified.", nameof(childRules));
            }

            _childRules.AddRange(rulesList);
            CompositionMode = compositionMode;

            if (compositionMode == CompositionMode.Threshold)
            {
                if (!passingThreshold.HasValue)
                {
                    throw new ArgumentNullException(nameof(passingThreshold),
                        "A passing threshold must be specified when using Threshold composition mode.");
                }

                if (passingThreshold.Value <= 0 || passingThreshold.Value > rulesList.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(passingThreshold),
                        $"Passing threshold must be between 1 and {rulesList.Count}.");
                }

                PassingThreshold = passingThreshold.Value;
            }
        }

        /// <summary>
        /// Validates the specified input against this composite rule by validating all child rules.
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

            var childResults = new List<ValidationResult>();

            // Validate all child rules
            foreach (var rule in _childRules)
            {
                try
                {
                    context.CancellationToken.ThrowIfCancellationRequested();
                    var result = rule.Validate(input, context);
                    childResults.Add(result);

                    // For Any mode, we can return early on the first success
                    if (CompositionMode == CompositionMode.Any && result.IsValid)
                    {
                        var success = ValidationResult.Success(this, new Dictionary<string, object>
                        {
                            { "ChildResults", childResults }
                        });
                        context.RecordRuleResult(Id, success);
                        return success;
                    }

                    // For All mode, we can return early on the first failure
                    if (CompositionMode == CompositionMode.All && !result.IsValid)
                    {
                        var failure = ValidationResult.Failure(this, $"{ErrorMessage} (Failed at child rule: {rule.Name})",
                            null, Severity, new Dictionary<string, object>
                            {
                                { "ChildResults", childResults },
                                { "FailedRule", rule.Id }
                            });
                        context.RecordRuleResult(Id, failure);
                        return failure;
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    var error = ValidationResult.Failure(this, $"Error executing child rule '{rule.Name}': {ex.Message}",
                        ex, Severity, new Dictionary<string, object>
                        {
                            { "ChildResults", childResults },
                            { "FailedRule", rule.Id }
                        });
                    context.RecordRuleResult(Id, error);
                    return error;
                }
            }

            // Evaluate the combined result based on composition mode
            bool isValid = CompositionMode switch
            {
                CompositionMode.All => childResults.All(r => r.IsValid),
                CompositionMode.Any => childResults.Any(r => r.IsValid),
                CompositionMode.Threshold => childResults.Count(r => r.IsValid) >= PassingThreshold,
                _ => throw new NotSupportedException($"Composition mode '{CompositionMode}' is not supported.")
            };

            ValidationResult finalResult;
            if (isValid)
            {
                finalResult = ValidationResult.Success(this, new Dictionary<string, object>
                {
                    { "ChildResults", childResults }
                });
            }
            else
            {
                string detailedMessage = CompositionMode switch
                {
                    CompositionMode.All => $"{ErrorMessage} (Not all child rules passed)",
                    CompositionMode.Any => $"{ErrorMessage} (No child rules passed)",
                    CompositionMode.Threshold => $"{ErrorMessage} (Less than {PassingThreshold} child rules passed)",
                    _ => ErrorMessage
                };

                finalResult = ValidationResult.Failure(this, detailedMessage, null, Severity, new Dictionary<string, object>
                {
                    { "ChildResults", childResults },
                    { "PassedCount", childResults.Count(r => r.IsValid) },
                    { "TotalCount", childResults.Count }
                });
            }

            context.RecordRuleResult(Id, finalResult);
            return finalResult;
        }

        /// <summary>
        /// Asynchronously validates the specified input against this composite rule by validating all child rules.
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

            var childResults = new List<ValidationResult>();

            // Validate all child rules
            foreach (var rule in _childRules)
            {
                try
                {
                    context.CancellationToken.ThrowIfCancellationRequested();
                    var result = await rule.ValidateAsync(input, context);
                    childResults.Add(result);

                    // For Any mode, we can return early on the first success
                    if (CompositionMode == CompositionMode.Any && result.IsValid)
                    {
                        var success = ValidationResult.Success(this, new Dictionary<string, object>
                        {
                            { "ChildResults", childResults }
                        });
                        context.RecordRuleResult(Id, success);
                        return success;
                    }

                    // For All mode, we can return early on the first failure
                    if (CompositionMode == CompositionMode.All && !result.IsValid)
                    {
                        var failure = ValidationResult.Failure(this, $"{ErrorMessage} (Failed at child rule: {rule.Name})",
                            null, Severity, new Dictionary<string, object>
                            {
                                { "ChildResults", childResults },
                                { "FailedRule", rule.Id }
                            });
                        context.RecordRuleResult(Id, failure);
                        return failure;
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    var error = ValidationResult.Failure(this, $"Error executing child rule '{rule.Name}': {ex.Message}",
                        ex, Severity, new Dictionary<string, object>
                        {
                            { "ChildResults", childResults },
                            { "FailedRule", rule.Id }
                        });
                    context.RecordRuleResult(Id, error);
                    return error;
                }
            }

            // Evaluate the combined result based on composition mode
            bool isValid = CompositionMode switch
            {
                CompositionMode.All => childResults.All(r => r.IsValid),
                CompositionMode.Any => childResults.Any(r => r.IsValid),
                CompositionMode.Threshold => childResults.Count(r => r.IsValid) >= PassingThreshold,
                _ => throw new NotSupportedException($"Composition mode '{CompositionMode}' is not supported.")
            };

            ValidationResult finalResult;
            if (isValid)
            {
                finalResult = ValidationResult.Success(this, new Dictionary<string, object>
                {
                    { "ChildResults", childResults }
                });
            }
            else
            {
                string detailedMessage = CompositionMode switch
                {
                    CompositionMode.All => $"{ErrorMessage} (Not all child rules passed)",
                    CompositionMode.Any => $"{ErrorMessage} (No child rules passed)",
                    CompositionMode.Threshold => $"{ErrorMessage} (Less than {PassingThreshold} child rules passed)",
                    _ => ErrorMessage
                };

                finalResult = ValidationResult.Failure(this, detailedMessage, null, Severity, new Dictionary<string, object>
                {
                    { "ChildResults", childResults },
                    { "PassedCount", childResults.Count(r => r.IsValid) },
                    { "TotalCount", childResults.Count }
                });
            }

            context.RecordRuleResult(Id, finalResult);
            return finalResult;
        }
    }
}
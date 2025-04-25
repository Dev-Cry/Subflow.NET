using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Ruleflow.NET.Engine.Models.ValidationResults;

namespace Ruleflow.NET.Engine.Models.Context
{
    /// <summary>
    /// Provides context for rule validation operations, including shared state and
    /// results of previous rule evaluations.
    /// </summary>
    public class RuleContext
    {
        private readonly ConcurrentDictionary<string, ValidationResult> _ruleResults = new();
        private readonly ConcurrentDictionary<string, object> _properties = new();

        /// <summary>
        /// Gets a cancellation token that can be used to cancel the validation operation.
        /// </summary>
        public CancellationToken CancellationToken { get; }

        /// <summary>
        /// Gets a read-only view of all rule validation results recorded so far.
        /// </summary>
        public IReadOnlyDictionary<string, ValidationResult> RuleResults => _ruleResults;

        /// <summary>
        /// Gets a dictionary of properties that can be shared across rules during validation.
        /// </summary>
        public IDictionary<string, object> Properties => _properties;

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleContext"/> class.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        public RuleContext(CancellationToken cancellationToken = default)
        {
            CancellationToken = cancellationToken;
        }

        /// <summary>
        /// Records a rule validation result in this context.
        /// </summary>
        /// <param name="ruleId">The ID of the rule that was validated.</param>
        /// <param name="result">The validation result.</param>
        public void RecordRuleResult(string ruleId, ValidationResult result)
        {
            if (string.IsNullOrEmpty(ruleId))
            {
                throw new ArgumentException("Rule ID cannot be null or empty.", nameof(ruleId));
            }

            _ruleResults[ruleId] = result ?? throw new ArgumentNullException(nameof(result));
        }

        /// <summary>
        /// Gets the validation result for a specific rule.
        /// </summary>
        /// <param name="ruleId">The ID of the rule.</param>
        /// <returns>The validation result, or null if the rule hasn't been validated yet.</returns>
        public ValidationResult GetRuleResult(string ruleId)
        {
            return _ruleResults.TryGetValue(ruleId, out var result) ? result : null;
        }

        /// <summary>
        /// Checks if a rule has been successfully validated.
        /// </summary>
        /// <param name="ruleId">The ID of the rule.</param>
        /// <returns>True if the rule succeeded, false otherwise.</returns>
        public bool HasRuleSucceeded(string ruleId)
        {
            return _ruleResults.TryGetValue(ruleId, out var result) && result.IsValid;
        }

        /// <summary>
        /// Checks if a rule validation has failed.
        /// </summary>
        /// <param name="ruleId">The ID of the rule.</param>
        /// <returns>True if the rule failed, false otherwise.</returns>
        public bool HasRuleFailed(string ruleId)
        {
            return _ruleResults.TryGetValue(ruleId, out var result) && !result.IsValid;
        }

        /// <summary>
        /// Gets or sets a property value in the context.
        /// </summary>
        /// <typeparam name="T">The type of the property value.</typeparam>
        /// <param name="key">The property key.</param>
        /// <returns>The property value, or default if not found.</returns>
        public T GetProperty<T>(string key)
        {
            if (_properties.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }
            return default;
        }

        /// <summary>
        /// Sets a property value in the context.
        /// </summary>
        /// <typeparam name="T">The type of the property value.</typeparam>
        /// <param name="key">The property key.</param>
        /// <param name="value">The property value.</param>
        public void SetProperty<T>(string key, T value)
        {
            _properties[key] = value;
        }

        /// <summary>
        /// Checks if all specified rules have succeeded.
        /// </summary>
        /// <param name="ruleIds">The IDs of the rules to check.</param>
        /// <returns>True if all rules succeeded, false otherwise.</returns>
        public bool AllRulesSucceeded(IEnumerable<string> ruleIds)
        {
            return ruleIds.All(HasRuleSucceeded);
        }

        /// <summary>
        /// Checks if any of the specified rules have succeeded.
        /// </summary>
        /// <param name="ruleIds">The IDs of the rules to check.</param>
        /// <returns>True if any rule succeeded, false otherwise.</returns>
        public bool AnyRuleSucceeded(IEnumerable<string> ruleIds)
        {
            return ruleIds.Any(HasRuleSucceeded);
        }

        /// <summary>
        /// Checks if all specified rules have failed.
        /// </summary>
        /// <param name="ruleIds">The IDs of the rules to check.</param>
        /// <returns>True if all rules failed, false otherwise.</returns>
        public bool AllRulesFailed(IEnumerable<string> ruleIds)
        {
            return ruleIds.All(HasRuleFailed);
        }

        /// <summary>
        /// Checks if any of the specified rules have failed.
        /// </summary>
        /// <param name="ruleIds">The IDs of the rules to check.</param>
        /// <returns>True if any rule failed, false otherwise.</returns>
        public bool AnyRuleFailed(IEnumerable<string> ruleIds)
        {
            return ruleIds.Any(HasRuleFailed);
        }

        /// <summary>
        /// Clears all recorded rule results and properties.
        /// </summary>
        public void Reset()
        {
            _ruleResults.Clear();
            _properties.Clear();
        }
    }
}
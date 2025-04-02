using Subflow.NET.Engine.Validation.Interfaces;
using System;

namespace Subflow.NET.Engine.Validation
{
    /// <summary>
    /// Základní třída pro validační pravidla s jednoznačným identifikátorem
    /// </summary>
    public abstract class IdentifiableValidationRule<T> : BaseValidationRule<T>, IIdentifiableValidationRule<T>
    {
        protected IdentifiableValidationRule(string ruleId)
        {
            RuleId = ruleId ?? throw new ArgumentNullException(nameof(ruleId));
        }

        /// <summary>
        /// Jedinečný identifikátor pravidla
        /// </summary>
        public string RuleId { get; }
    }
}
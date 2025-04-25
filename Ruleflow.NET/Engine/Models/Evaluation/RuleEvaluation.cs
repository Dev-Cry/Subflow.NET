using System;
using System.Collections.Generic;
using System.Text;
using Ruleflow.NET.Engine.Models.Rule;

namespace Ruleflow.NET.Engine.Models.Evaluation
{
    /// <summary>
    /// Uchovává výsledek vyhodnocení sdíleného pravidla pro daný vstup.
    /// </summary>
    /// <typeparam name="TInput">Typ validovaných dat.</typeparam>
    public class RuleEvaluation<TInput>(
        SharedRule<TInput> sharedRule,
        TInput input,
        bool passed,
        Dictionary<string, object>? details = null,
        DateTimeOffset? evaluatedAt = null)
    {
        public Guid EvaluationId { get; } = Guid.NewGuid();
        /// <summary>
        /// Reference na sdílené pravidlo, které bylo vyhodnoceno.
        /// </summary>
        public SharedRule<TInput> SharedRule { get; } = sharedRule ?? throw new ArgumentNullException(nameof(sharedRule));
        /// <summary>
        /// Původní vstup, který byl předán do pravidla.
        /// </summary>
        public TInput Input { get; } = input;
        /// <summary>
        /// Výsledek vyhodnocení (true = pravidlo prošlo).
        /// </summary>
        public bool Passed { get; } = passed;
        /// <summary>
        /// Detailní informace z vyhodnocení (např. chybové hlášky, mezivýsledky).
        /// </summary>
        public IReadOnlyDictionary<string, object>? Details { get; } = details;
        /// <summary>
        /// Čas provedení vyhodnocení (UTC).
        /// </summary>
        public DateTimeOffset EvaluatedAt { get; } = evaluatedAt?.ToUniversalTime() ?? DateTimeOffset.UtcNow;

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"RuleEvaluation(Id={EvaluationId}, Rule={SharedRule.Rule.RuleId}, Passed={Passed}");
            sb.Append($", Priority={SharedRule.Rule.Priority}");
            sb.Append($", IsActive={SharedRule.Rule.IsActive}");
            sb.Append($", ContextId={SharedRule.Context.ContextId}");
            sb.Append($", EvaluatedAt=\"{EvaluatedAt:o}\"");
            if (Details != null && Details.Count > 0)
                sb.Append($", DetailsCount={Details.Count}");
            sb.Append(")");
            return sb.ToString();
        }
    }
}
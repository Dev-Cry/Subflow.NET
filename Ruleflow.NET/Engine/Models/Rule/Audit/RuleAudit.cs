using System;
using System.Collections.Generic;
using System.Text;
using Ruleflow.NET.Engine.Models.Evaluation;
using Ruleflow.NET.Engine.Models.Shared;

namespace Ruleflow.NET.Engine.Models.Rule.Audit
{
    /// <summary>
    /// Audituje sdílené pravidlo (SharedRule) spolu s výsledky jeho vyhodnocení.
    /// </summary>
    /// <typeparam name="TInput">Typ validovaných vstupních dat.</typeparam>
    /// <remarks>
    /// Konstruktor vytvářející auditní záznam pro sdílené pravidlo.
    /// </remarks>
    /// <param name="sharedRule">Sdílené pravidlo včetně kontextu.</param>
    /// <param name="evaluationResult">Výsledek vyhodnocení.</param>
    /// <param name="performedBy">Kdo audit provedl.</param>
    /// <param name="correlationId">Volitelné korelační ID.</param>
    /// <param name="metadata">Dodatečná metadata.</param>
    /// <param name="auditedAt">Čas auditu (UTC).</param>
    public class RuleAudit<TInput>(
        SharedRule<TInput> sharedRule,
        RuleEvaluationResult<TInput> evaluationResult,
        string? performedBy = null,
        string? correlationId = null,
        IEnumerable<KeyValuePair<string, object>>? metadata = null,
        DateTimeOffset? auditedAt = null)
    {
        /// <summary>
        /// Jedinečný identifikátor auditního záznamu.
        /// </summary>
        public Guid AuditId { get; } = Guid.NewGuid();

        /// <summary>
        /// Sdílené pravidlo včetně jeho kontextu.
        /// </summary>
        public SharedRule<TInput> SharedRule { get; } = sharedRule ?? throw new ArgumentNullException(nameof(sharedRule));

        /// <summary>
        /// Výsledek vyhodnocení pravidla.
        /// </summary>
        public RuleEvaluationResult<TInput> EvaluationResult { get; } = evaluationResult ?? throw new ArgumentNullException(nameof(evaluationResult));

        /// <summary>
        /// Identifikace volajícího (uživatel, služba apod.).
        /// </summary>
        public string? PerformedBy { get; } = performedBy;

        /// <summary>
        /// Volitelné korelační ID pro sledování napříč systémy.
        /// </summary>
        public string? CorrelationId { get; } = correlationId;

        /// <summary>
        /// Dodatečná metadata k auditu.
        /// </summary>
        private readonly Dictionary<string, object> _metadata = metadata != null
                ? new Dictionary<string, object>(metadata)
                : new Dictionary<string, object>();
        public IReadOnlyDictionary<string, object> Metadata => _metadata;

        /// <summary>
        /// Čas provedení auditu.
        /// </summary>
        public DateTimeOffset AuditedAt { get; } = auditedAt?.ToUniversalTime() ?? DateTimeOffset.UtcNow;

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"RuleAudit(Id={AuditId}, RuleId=\"{SharedRule.Rule.RuleId}\"");
            sb.Append($", Success={EvaluationResult.IsSuccess}");
            if (!string.IsNullOrEmpty(PerformedBy)) sb.Append($", By=\"{PerformedBy}\"");
            if (!string.IsNullOrEmpty(CorrelationId)) sb.Append($", CorrelationId=\"{CorrelationId}\"");
            sb.Append($", MetadataCount={_metadata.Count}");
            sb.Append($", AuditedAt=\"{AuditedAt:o}\"");
            sb.Append(")");
            return sb.ToString();
        }
    }
}

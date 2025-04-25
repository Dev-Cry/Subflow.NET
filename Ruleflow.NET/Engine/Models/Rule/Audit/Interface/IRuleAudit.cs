using Ruleflow.NET.Engine.Models.Evaluation.Interface;
using Ruleflow.NET.Engine.Models.Shared.Interface;
using System;
using System.Collections.Generic;

namespace Ruleflow.NET.Engine.Models.Rule.Audit.Interface
{
    /// <summary>
    /// Rozhraní pro auditování pravidel.
    /// </summary>
    /// <typeparam name="TInput">Typ validovaných dat.</typeparam>
    public interface IRuleAudit<TInput>
    {
        /// <summary>
        /// Unikátní identifikátor auditu.
        /// </summary>
        Guid AuditId { get; }

        /// <summary>
        /// Sdílené pravidlo, které bylo auditováno.
        /// </summary>
        ISharedRule<TInput> SharedRule { get; }

        /// <summary>
        /// Výsledek vyhodnocení pravidla.
        /// </summary>
        IRuleEvaluationResult<TInput> EvaluationResult { get; }

        /// <summary>
        /// Identifikace volajícího.
        /// </summary>
        string? PerformedBy { get; }

        /// <summary>
        /// Korelační ID pro sledování napříč systémy.
        /// </summary>
        string? CorrelationId { get; }

        /// <summary>
        /// Metadata auditu.
        /// </summary>
        IReadOnlyDictionary<string, object> Metadata { get; }

        /// <summary>
        /// Čas provedení auditu.
        /// </summary>
        DateTimeOffset AuditedAt { get; }
    }
}
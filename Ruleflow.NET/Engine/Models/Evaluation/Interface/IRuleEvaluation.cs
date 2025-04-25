using Ruleflow.NET.Engine.Models.Shared;
using Ruleflow.NET.Engine.Models.Shared.Interface;

namespace Ruleflow.NET.Engine.Models.Evaluation.Interface
{
    /// <summary>
    /// Rozhraní pro informace o vyhodnocení sdíleného pravidla.
    /// </summary>
    /// <typeparam name="TInput">Typ validovaných vstupních dat.</typeparam>
    public interface IRuleEvaluation<TInput>
    {
        /// <summary>
        /// Unikátní identifikátor vyhodnocení.
        /// </summary>
        Guid EvaluationId { get; }

        /// <summary>
        /// Sdílené pravidlo, které bylo vyhodnoceno.
        /// </summary>
        ISharedRule<TInput> SharedRule { get; }

        /// <summary>
        /// Vstupní data.
        /// </summary>
        TInput Input { get; }

        /// <summary>
        /// Výsledek vyhodnocení (true = pravidlo prošlo).
        /// </summary>
        bool Passed { get; }

        /// <summary>
        /// Detailní informace z vyhodnocení.
        /// </summary>
        IReadOnlyDictionary<string, object>? Details { get; }

        /// <summary>
        /// Čas provedení vyhodnocení.
        /// </summary>
        DateTimeOffset EvaluatedAt { get; }
    }
}
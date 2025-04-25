using Ruleflow.NET.Engine.Models.Rule.Context.Interface;
using Ruleflow.NET.Engine.Models.Rule.Interface;

namespace Ruleflow.NET.Engine.Models.Evaluation.Interface
{
    /// <summary>
    /// Rozhraní pro výsledek vyhodnocení pravidla.
    /// </summary>
    /// <typeparam name="TInput">Typ validovaných vstupních dat.</typeparam>
    public interface IRuleEvaluationResult<TInput>
    {
        /// <summary>
        /// Pravidlo, které bylo vyhodnoceno.
        /// </summary>
        IRule<TInput> Rule { get; }

        /// <summary>
        /// Kontext použitý při vyhodnocení.
        /// </summary>
        IRuleContext Context { get; }

        /// <summary>
        /// Vstupní data, proti kterým bylo pravidlo aplikováno.
        /// </summary>
        TInput Input { get; }

        /// <summary>
        /// Indikuje, zda pravidlo prošlo.
        /// </summary>
        bool IsSuccess { get; }

        /// <summary>
        /// Seznam zpráv a chyb vzniklých při vyhodnocení.
        /// </summary>
        IReadOnlyList<string> Messages { get; }

        /// <summary>
        /// Čas vyhodnocení.
        /// </summary>
        DateTimeOffset EvaluatedAt { get; }
    }
}
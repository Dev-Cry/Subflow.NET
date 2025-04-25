using Ruleflow.NET.Engine.Models.Evaluation;
using Ruleflow.NET.Engine.Models.Rule.Context;
using Ruleflow.NET.Engine.Models.Rule.Type;
using Ruleflow.NET.Engine.Models.Rule.Type.Interface;

namespace Ruleflow.NET.Engine.Models.Rule.Interface
{
    /// <summary>
    /// Základní rozhraní pro všechna validační pravidla.
    /// </summary>
    /// <typeparam name="TInput">Typ validovaných dat.</typeparam>
    public interface IRule<TInput>
    {
        /// <summary>
        /// Jedinečný identifikátor pravidla.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Řetězcový identifikátor pravidla (GUID).
        /// </summary>
        string RuleId { get; }

        /// <summary>
        /// Název pravidla.
        /// </summary>
        string? Name { get; }

        /// <summary>
        /// Popis pravidla.
        /// </summary>
        string? Description { get; }

        /// <summary>
        /// Priorita pravidla (vyšší číslo = vyšší priorita).
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Indikuje, zda je pravidlo aktivní.
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Časová značka vytvoření/aktualizace pravidla.
        /// </summary>
        DateTimeOffset Timestamp { get; }

        /// <summary>
        /// Identifikátor typu pravidla.
        /// </summary>
        int RuleTypeId { get; }

        /// <summary>
        /// Typ pravidla.
        /// </summary>
        IRuleType Type { get; }

        /// <summary>
        /// Vyhodnotí pravidlo proti zadaným vstupním datům.
        /// </summary>
        /// <param name="input">Vstupní data pro validaci.</param>
        /// <param name="context">Kontext vyhodnocení pravidla.</param>
        /// <returns>Výsledek vyhodnocení pravidla.</returns>
        RuleEvaluationResult<TInput> Evaluate(TInput input, RuleContext context);
    }
}
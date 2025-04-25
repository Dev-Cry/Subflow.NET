using Ruleflow.NET.Engine.Models.Rule.Type.Interface;

namespace Ruleflow.NET.Engine.Models.Rule.Interface
{
    /// <summary>
    /// Základní rozhraní pro pravidlo s typovou bezpečností.
    /// </summary>
    /// <typeparam name="TInput">Typ dat, která budou validována.</typeparam>
    public interface IRule<TInput>
    {
        /// <summary>
        /// Unikátní identifikátor pravidla v databázi.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Veřejný identifikátor pravidla pro použití v kódu.
        /// </summary>
        string RuleId { get; }

        /// <summary>
        /// Volitelný název pravidla.
        /// </summary>
        string? Name { get; }

        /// <summary>
        /// Volitelný popis pravidla.
        /// </summary>
        string? Description { get; }

        /// <summary>
        /// Priorita pravidla. Vyšší číslo = vyšší priorita.
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Indikuje, zda je pravidlo aktivní.
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Časová značka vytvoření nebo poslední aktualizace pravidla.
        /// </summary>
        DateTimeOffset Timestamp { get; }

        /// <summary>
        /// ID typu pravidla.
        /// </summary>
        int RuleTypeId { get; }

        /// <summary>
        /// Odkaz na typ pravidla.
        /// </summary>
        IRuleType Type { get; }
    }
}
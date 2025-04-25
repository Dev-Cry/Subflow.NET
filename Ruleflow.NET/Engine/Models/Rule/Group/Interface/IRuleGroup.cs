using Ruleflow.NET.Engine.Models.Rule.Group.Interface;
using Ruleflow.NET.Engine.Models.Rule.Interface;

namespace Ruleflow.NET.Engine.Models.Rule.Group.Interface
{
    /// <summary>
    /// Rozhraní pro skupinu pravidel.
    /// </summary>
    /// <typeparam name="TInput">Typ validovaných dat.</typeparam>
    public interface IRuleGroup<TInput>
    {
        /// <summary>
        /// Unikátní identifikátor skupiny v databázi.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Veřejný identifikátor skupiny pro použití v kódu.
        /// </summary>
        string GroupId { get; }

        /// <summary>
        /// Volitelný název skupiny.
        /// </summary>
        string? Name { get; }

        /// <summary>
        /// Volitelný popis skupiny.
        /// </summary>
        string? Description { get; }

        /// <summary>
        /// Indikuje, zda je skupina aktivní.
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Časová značka vytvoření nebo poslední aktualizace skupiny.
        /// </summary>
        DateTimeOffset Timestamp { get; }

        /// <summary>
        /// ID typu skupiny.
        /// </summary>
        int RuleGroupTypeId { get; }

        /// <summary>
        /// Odkaz na typ skupiny.
        /// </summary>
        IRuleGroupType Type { get; }

        /// <summary>
        /// Pravidla ve skupině.
        /// </summary>
        IReadOnlyList<IRule<TInput>> Rules { get; }

        /// <summary>
        /// Přidá pravidlo do skupiny.
        /// </summary>
        void AddRule(IRule<TInput> rule);

        /// <summary>
        /// Odebere pravidlo ze skupiny.
        /// </summary>
        bool RemoveRule(IRule<TInput> rule);
    }
}
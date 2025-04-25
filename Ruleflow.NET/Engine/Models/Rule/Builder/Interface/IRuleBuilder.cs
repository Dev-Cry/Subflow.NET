using Ruleflow.NET.Engine.Models.Rule;
using Ruleflow.NET.Engine.Models.Rule.Context;
using Ruleflow.NET.Engine.Models.Rule.Type;

namespace Ruleflow.NET.Engine.Models.Rule.Builder.Interface
{
    /// <summary>
    /// Základní rozhraní pro buildery pravidel.
    /// </summary>
    /// <typeparam name="TInput">Typ validovaných dat.</typeparam>
    /// <typeparam name="TBuilder">Konkrétní typ builderu (pro metodu fluent řetězení).</typeparam>
    public interface IRuleBuilder<TInput, TBuilder>
        where TBuilder : IRuleBuilder<TInput, TBuilder>
    {
        /// <summary>
        /// Nastaví název pravidla.
        /// </summary>
        /// <param name="name">Název pravidla.</param>
        /// <returns>Instance builderu pro řetězení.</returns>
        TBuilder WithName(string name);

        /// <summary>
        /// Nastaví popis pravidla.
        /// </summary>
        /// <param name="description">Popis pravidla.</param>
        /// <returns>Instance builderu pro řetězení.</returns>
        TBuilder WithDescription(string description);

        /// <summary>
        /// Nastaví prioritu pravidla.
        /// </summary>
        /// <param name="priority">Priorita pravidla.</param>
        /// <returns>Instance builderu pro řetězení.</returns>
        TBuilder WithPriority(int priority);

        /// <summary>
        /// Nastaví, zda je pravidlo aktivní.
        /// </summary>
        /// <param name="isActive">Aktivita pravidla.</param>
        /// <returns>Instance builderu pro řetězení.</returns>
        TBuilder SetActive(bool isActive);

        /// <summary>
        /// Nastaví časovou značku pravidla.
        /// </summary>
        /// <param name="timestamp">Časová značka.</param>
        /// <returns>Instance builderu pro řetězení.</returns>
        TBuilder WithTimestamp(DateTimeOffset timestamp);

        /// <summary>
        /// Nastaví typ pravidla.
        /// </summary>
        /// <param name="type">Typ pravidla.</param>
        /// <returns>Instance builderu pro řetězení.</returns>
        TBuilder WithType(RuleType type);

        /// <summary>
        /// Sestaví a vrátí pravidlo podle nastavené konfigurace.
        /// </summary>
        /// <returns>Vytvořené pravidlo.</returns>
        Rule<TInput> Build();
    }
}
using Ruleflow.NET.Engine.Models.Rule.Context;

using Ruleflow.NET.Engine.Models.Rule.Interface;

/// <summary>
/// Rozhraní pro přepínací pravidlo, které vybírá jednu z mnoha možností.
/// </summary>
/// <typeparam name="TInput">Typ validovaných dat.</typeparam>
public interface ISwitchRule<TInput> : IRule<TInput>
{
    /// <summary>
    /// Slovník případů s klíčem a odpovídajícím pravidlem.
    /// </summary>
    IReadOnlyDictionary<object, IRule<TInput>> Cases { get; }

    /// <summary>
    /// Výchozí pravidlo, které se použije, pokud žádný případ nevyhovuje.
    /// </summary>
    IRule<TInput>? DefaultCase { get; set; }

    /// <summary>
    /// Určí klíč pro přepínání mezi případy.
    /// </summary>
    /// <param name="input">Vstupní data pro validaci.</param>
    /// <param name="context">Kontext vyhodnocení pravidla.</param>
    /// <returns>Klíč pro výběr případu.</returns>
    object GetSwitchKey(TInput input, RuleContext context);

    /// <summary>
    /// Přidá případ do přepínače.
    /// </summary>
    /// <param name="key">Klíč případu.</param>
    /// <param name="rule">Pravidlo odpovídající případu.</param>
    void AddCase(object key, IRule<TInput> rule);
}
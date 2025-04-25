using Ruleflow.NET.Engine.Models.Rule.Context;

using Ruleflow.NET.Engine.Models.Rule.Interface;

/// <summary>
/// Rozhraní pro pravidlo s jednou odpovědností (jednoduché pravidlo).
/// </summary>
/// <typeparam name="TInput">Typ validovaných dat.</typeparam>
public interface ISingleResponsibilityRule<TInput> : IRule<TInput>
{
    /// <summary>
    /// Jednoduchá validační funkce, která ověřuje jednu vlastnost/podmínku.
    /// </summary>
    /// <param name="input">Vstupní data pro validaci.</param>
    /// <param name="context">Kontext vyhodnocení pravidla.</param>
    /// <returns>True pokud validace prošla, jinak false.</returns>
    bool Validate(TInput input, RuleContext context);
}
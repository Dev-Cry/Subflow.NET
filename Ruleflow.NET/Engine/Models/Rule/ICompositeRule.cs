using Ruleflow.NET.Engine.Models.Rule.Interface;

/// <summary>
/// Rozhraní pro kompozitní pravidlo, které sdružuje více pravidel.
/// </summary>
/// <typeparam name="TInput">Typ validovaných dat.</typeparam>
public interface ICompositeRule<TInput> : IRule<TInput>
{
    /// <summary>
    /// Seznam vnořených pravidel.
    /// </summary>
    IReadOnlyList<IRule<TInput>> ChildRules { get; }

    /// <summary>
    /// Přidá pravidlo do kompozitu.
    /// </summary>
    /// <param name="rule">Pravidlo, které bude přidáno do kompozitu.</param>
    void AddRule(IRule<TInput> rule);

    /// <summary>
    /// Odebere pravidlo z kompozitu.
    /// </summary>
    /// <param name="rule">Pravidlo, které bude odebráno z kompozitu.</param>
    /// <returns>True, pokud bylo pravidlo úspěšně odebráno, jinak false.</returns>
    bool RemoveRule(IRule<TInput> rule);
}
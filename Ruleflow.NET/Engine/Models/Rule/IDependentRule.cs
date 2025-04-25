using Ruleflow.NET.Engine.Models.Rule.Interface;

/// <summary>
/// Rozhraní pro pravidlo závislé na výsledcích jiných pravidel.
/// </summary>
/// <typeparam name="TInput">Typ validovaných dat.</typeparam>
public interface IDependentRule<TInput> : IRule<TInput>
{
    /// <summary>
    /// Pravidla, na kterých závisí výsledek tohoto pravidla.
    /// </summary>
    IReadOnlyList<IRule<TInput>> Dependencies { get; }

    /// <summary>
    /// Přidá závislost na jiném pravidle.
    /// </summary>
    /// <param name="rule">Pravidlo, na kterém závisí toto pravidlo.</param>
    void AddDependency(IRule<TInput> rule);
}
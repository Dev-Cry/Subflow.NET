using Ruleflow.NET.Engine.Models.Rule.Context;

using Ruleflow.NET.Engine.Models.Rule.Interface;

/// <summary>
/// Rozhraní pro podmínkové pravidlo, které volí různé větve na základě podmínky.
/// </summary>
/// <typeparam name="TInput">Typ validovaných dat.</typeparam>
public interface IConditionalRule<TInput> : IRule<TInput>
{
    /// <summary>
    /// Pravidlo, které se vyhodnotí při splnění podmínky.
    /// </summary>
    IRule<TInput>? ThenRule { get; }

    /// <summary>
    /// Pravidlo, které se vyhodnotí při nesplnění podmínky.
    /// </summary>
    IRule<TInput>? ElseRule { get; }

    /// <summary>
    /// Podmínka, která určuje, která větev bude vyhodnocena.
    /// </summary>
    /// <param name="input">Vstupní data pro validaci.</param>
    /// <param name="context">Kontext vyhodnocení pravidla.</param>
    /// <returns>True pro Then větev, False pro Else větev.</returns>
    bool EvaluateCondition(TInput input, RuleContext context);
}
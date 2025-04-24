using Ruleflow.NET.Engine.Validation.Builders;

using Ruleflow.NET.Engine.Validation.Interfaces;

/// <summary>
/// Rozhraní pro definici else větve podmíněných validačních pravidel.
/// </summary>
/// <typeparam name="T">Typ validovaných dat</typeparam>
public interface IRuleConditionElseBuilder<T>
{
    /// <summary>
    /// Definuje podmínku pro else-if větev validačního pravidla.
    /// </summary>
    /// <param name="condition">Podmínka pro vyhodnocení</param>
    /// <returns>Builder pro konfiguraci else-if větve</returns>
    IRuleConditionThenBuilder<T> ElseIf(Func<T, bool> condition);

    /// <summary>
    /// Definuje validační pravidlo, které se provede, když žádná z předchozích podmínek není splněna.
    /// </summary>
    /// <param name="configureRule">Akce pro konfiguraci pravidla</param>
    /// <returns>Validační pravidlo</returns>
    IValidationRule<T> Else(Action<ValidationRuleBuilder<T>> configureRule);

    /// <summary>
    /// Vytvoří validační pravidlo z aktuální konfigurace.
    /// </summary>
    /// <returns>Validační pravidlo</returns>
    IValidationRule<T> Build();
}
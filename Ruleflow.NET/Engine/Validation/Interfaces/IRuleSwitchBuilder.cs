using Ruleflow.NET.Engine.Validation.Builders;

using Ruleflow.NET.Engine.Validation.Interfaces;

/// <summary>
/// Rozhraní pro definici validačních pravidel pomocí switch konstrukce.
/// </summary>
/// <typeparam name="T">Typ validovaných dat</typeparam>
/// <typeparam name="TValue">Typ hodnoty pro switch</typeparam>
public interface IRuleSwitchBuilder<T, TValue>
{
    /// <summary>
    /// Definuje větev pro konkrétní hodnotu.
    /// </summary>
    /// <param name="value">Hodnota pro porovnání</param>
    /// <param name="configureRule">Akce pro konfiguraci pravidla</param>
    /// <returns>Builder pro možnost definice dalších case větví</returns>
    IRuleSwitchBuilder<T, TValue> Case(TValue value, Action<ValidationRuleBuilder<T>> configureRule);

    /// <summary>
    /// Definuje výchozí větev, která se provede, když žádná z case hodnot neodpovídá.
    /// </summary>
    /// <param name="configureRule">Akce pro konfiguraci pravidla</param>
    /// <returns>Builder pro další konfiguraci</returns>
    IRuleSwitchBuilder<T, TValue> Default(Action<ValidationRuleBuilder<T>> configureRule);

    /// <summary>
    /// Vytvoří validační pravidlo z aktuální konfigurace.
    /// </summary>
    /// <returns>Validační pravidlo</returns>
    IValidationRule<T> Build();
}
}
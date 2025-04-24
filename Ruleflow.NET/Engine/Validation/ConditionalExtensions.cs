// Ruleflow.NET/Engine/Validation/ConditionalExtensions.cs
using Ruleflow.NET.Engine.Validation.Builders;
using Ruleflow.NET.Engine.Validation.Conditions;
using Ruleflow.NET.Engine.Validation.Interfaces;
using System;

namespace Ruleflow.NET.Engine.Validation
{
    /// <summary>
    /// Rozšiřující metody pro podmínkové konstrukce v Ruleflow.NET.
    /// </summary>
    public static class ConditionalExtensions
    {
        /// <summary>
        /// Vytvoří podmíněné validační pravidlo pomocí if/then konstrukce.
        /// </summary>
        /// <typeparam name="T">Typ validovaných dat</typeparam>
        /// <param name="builder">Builder validačního pravidla</param>
        /// <param name="condition">Podmínka pro vyhodnocení</param>
        /// <returns>Builder pro podmíněné validační pravidlo</returns>
        public static IRuleConditionBuilder<T> If<T>(
            this ValidationRuleBuilder<T> builder,
            Func<T, bool> condition)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));

            return new RuleConditionBuilder<T>(builder, condition);
        }

        /// <summary>
        /// Vytvoří validační pravidlo založené na switch konstrukci.
        /// </summary>
        /// <typeparam name="T">Typ validovaných dat</typeparam>
        /// <typeparam name="TValue">Typ hodnoty pro switch</typeparam>
        /// <param name="builder">Builder validačního pravidla</param>
        /// <param name="valueSelector">Funkce pro získání hodnoty pro switch</param>
        /// <returns>Builder pro switch validační pravidlo</returns>
        public static IRuleSwitchBuilder<T, TValue> Switch<T, TValue>(
            this ValidationRuleBuilder<T> builder,
            Func<T, TValue> valueSelector)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (valueSelector == null)
                throw new ArgumentNullException(nameof(valueSelector));

            return new RuleSwitchBuilder<T, TValue>(builder, valueSelector);
        }
    }
}

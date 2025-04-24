// Ruleflow.NET/Engine/Validation/Conditions/OperatorExtensions.cs
using Ruleflow.NET.Engine.Validation.Builders;
using System;

namespace Ruleflow.NET.Engine.Validation.Conditions
{
    /// <summary>
    /// Rozšiřující metody pro operátory (ternární, null-coalescing).
    /// </summary>
    public static class OperatorExtensions
    {
        /// <summary>
        /// Přidá validační akci využívající ternární operátor.
        /// </summary>
        /// <typeparam name="T">Typ validovaných dat</typeparam>
        /// <param name="builder">Builder validačního pravidla</param>
        /// <param name="condition">Podmínka pro vyhodnocení</param>
        /// <param name="whenTrue">Akce prováděná, když je podmínka splněna</param>
        /// <param name="whenFalse">Akce prováděná, když podmínka není splněna</param>
        /// <returns>Builder validačního pravidla</returns>
        public static ValidationRuleBuilder<T> WithTernaryAction<T>(
            this ValidationRuleBuilder<T> builder,
            Func<T, bool> condition,
            Action<T> whenTrue,
            Action<T> whenFalse)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));
            if (whenTrue == null)
                throw new ArgumentNullException(nameof(whenTrue));
            if (whenFalse == null)
                throw new ArgumentNullException(nameof(whenFalse));

            return builder.WithAction(input =>
            {
                if (condition(input))
                {
                    whenTrue(input);
                }
                else
                {
                    whenFalse(input);
                }
            });
        }

        /// <summary>
        /// Přidá validační akci využívající null-coalescing operátor pro referenční typy.
        /// </summary>
        /// <typeparam name="T">Typ validovaných dat</typeparam>
        /// <typeparam name="TValue">Typ hodnoty</typeparam>
        /// <param name="builder">Builder validačního pravidla</param>
        /// <param name="primarySelector">Funkce pro získání primární hodnoty</param>
        /// <param name="fallbackSelector">Funkce pro získání náhradní hodnoty</param>
        /// <param name="validation">Validační akce</param>
        /// <returns>Builder validačního pravidla</returns>
        public static ValidationRuleBuilder<T> WithNullCoalescingAction<T, TValue>(
            this ValidationRuleBuilder<T> builder,
            Func<T, TValue?> primarySelector,
            Func<T, TValue> fallbackSelector,
            Action<T, TValue> validation) where TValue : class
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (primarySelector == null)
                throw new ArgumentNullException(nameof(primarySelector));
            if (fallbackSelector == null)
                throw new ArgumentNullException(nameof(fallbackSelector));
            if (validation == null)
                throw new ArgumentNullException(nameof(validation));

            return builder.WithAction(input =>
            {
                var value = primarySelector(input) ?? fallbackSelector(input);
                validation(input, value);
            });
        }

        /// <summary>
        /// Přidá validační akci využívající null-coalescing operátor pro hodnoty nullable typů.
        /// </summary>
        /// <typeparam name="T">Typ validovaných dat</typeparam>
        /// <typeparam name="TValue">Typ hodnoty</typeparam>
        /// <param name="builder">Builder validačního pravidla</param>
        /// <param name="primarySelector">Funkce pro získání primární hodnoty</param>
        /// <param name="fallbackSelector">Funkce pro získání náhradní hodnoty</param>
        /// <param name="validation">Validační akce</param>
        /// <returns>Builder validačního pravidla</returns>
        public static ValidationRuleBuilder<T> WithNullCoalescingValueAction<T, TValue>(
            this ValidationRuleBuilder<T> builder,
            Func<T, TValue?> primarySelector,
            Func<T, TValue> fallbackSelector,
            Action<T, TValue> validation) where TValue : struct
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (primarySelector == null)
                throw new ArgumentNullException(nameof(primarySelector));
            if (fallbackSelector == null)
                throw new ArgumentNullException(nameof(fallbackSelector));
            if (validation == null)
                throw new ArgumentNullException(nameof(validation));

            return builder.WithAction(input =>
            {
                var value = primarySelector(input) ?? fallbackSelector(input);
                validation(input, value);
            });
        }
    }
}
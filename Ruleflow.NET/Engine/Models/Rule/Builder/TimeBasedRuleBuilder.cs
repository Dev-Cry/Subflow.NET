using Ruleflow.NET.Engine.Models.Rule.Builder.Interface;
using Ruleflow.NET.Engine.Models.Rule.Type;
using System;

namespace Ruleflow.NET.Engine.Models.Rule.Builder
{
    /// <summary>
    /// Builder pro vytváření časových pravidel.
    /// </summary>
    /// <typeparam name="TInput">Typ validovaných dat.</typeparam>
    public class TimeBasedRuleBuilder<TInput> : RuleBuilder<TInput, TimeBasedRuleBuilder<TInput>>
    {
        private TimeBasedRule<TInput>.TimeConditionDelegate? _timeConditionFunc;
        private bool _useCurrentTime = true;

        /// <summary>
        /// Vytvoří nový builder pro časové pravidlo.
        /// </summary>
        /// <param name="id">Identifikátor pravidla.</param>
        /// <param name="type">Typ pravidla.</param>
        public TimeBasedRuleBuilder(int id, RuleType type) : base(id, type)
        {
        }

        /// <summary>
        /// Nastaví funkci časové podmínky.
        /// </summary>
        /// <param name="timeConditionFunc">Funkce vyhodnocující časovou podmínku.</param>
        /// <returns>Instance builderu pro řetězení.</returns>
        public TimeBasedRuleBuilder<TInput> WithTimeCondition(TimeBasedRule<TInput>.TimeConditionDelegate timeConditionFunc)
        {
            _timeConditionFunc = timeConditionFunc ?? throw new ArgumentNullException(nameof(timeConditionFunc));
            return this;
        }

        /// <summary>
        /// Nastaví, zda se má pro vyhodnocení použít aktuální čas.
        /// </summary>
        /// <param name="useCurrentTime">True pro použití aktuálního času, false pro čas z kontextu.</param>
        /// <returns>Instance builderu pro řetězení.</returns>
        public TimeBasedRuleBuilder<TInput> UseCurrentTime(bool useCurrentTime)
        {
            _useCurrentTime = useCurrentTime;
            return this;
        }

        /// <summary>
        /// Sestaví a vrátí časové pravidlo podle nastavené konfigurace.
        /// </summary>
        /// <returns>Vytvořené časové pravidlo.</returns>
        public override Rule<TInput> Build()
        {
            if (_timeConditionFunc == null)
                throw new InvalidOperationException("Časové pravidlo vyžaduje funkci pro vyhodnocení časové podmínky.");

            return new TimeBasedRule<TInput>(
                Id,
                Type,
                _timeConditionFunc,
                _useCurrentTime,
                RuleId,
                Name,
                Description,
                Priority,
                IsActive,
                Timestamp);
        }
    }
}
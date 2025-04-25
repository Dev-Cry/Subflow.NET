using Ruleflow.NET.Engine.Models.Rule;
using Ruleflow.NET.Engine.Models.Rule.Builder.Interface;
using Ruleflow.NET.Engine.Models.Rule.Interface;
using Ruleflow.NET.Engine.Models.Rule.Type;

namespace Ruleflow.NET.Engine.Models.Rule.Builder
{
    /// <summary>
    /// Builder pro vytváření podmínkových pravidel.
    /// </summary>
    /// <typeparam name="TInput">Typ validovaných dat.</typeparam>
    public class ConditionalRuleBuilder<TInput> : RuleBuilder<TInput, ConditionalRuleBuilder<TInput>>
    {
        private ConditionalRule<TInput>.ConditionDelegate? _conditionFunc;
        private IRule<TInput>? _thenRule;
        private IRule<TInput>? _elseRule;

        /// <summary>
        /// Vytvoří nový builder pro podmínkové pravidlo.
        /// </summary>
        /// <param name="id">ID pravidla.</param>
        /// <param name="type">Typ pravidla.</param>
        public ConditionalRuleBuilder(int id, RuleType type) : base(id, type)
        {
        }

        /// <summary>
        /// Nastaví podmínkovou funkci pravidla.
        /// </summary>
        /// <param name="conditionFunc">Funkce pro vyhodnocení podmínky.</param>
        /// <returns>Instance builderu pro řetězení.</returns>
        public ConditionalRuleBuilder<TInput> WithCondition(ConditionalRule<TInput>.ConditionDelegate conditionFunc)
        {
            _conditionFunc = conditionFunc;
            return this;
        }

        /// <summary>
        /// Nastaví pravidlo pro větev THEN (když je podmínka splněna).
        /// </summary>
        /// <param name="thenRule">Pravidlo pro větev THEN.</param>
        /// <returns>Instance builderu pro řetězení.</returns>
        public ConditionalRuleBuilder<TInput> WithThenRule(IRule<TInput> thenRule)
        {
            _thenRule = thenRule;
            return this;
        }

        /// <summary>
        /// Nastaví pravidlo pro větev ELSE (když podmínka není splněna).
        /// </summary>
        /// <param name="elseRule">Pravidlo pro větev ELSE.</param>
        /// <returns>Instance builderu pro řetězení.</returns>
        public ConditionalRuleBuilder<TInput> WithElseRule(IRule<TInput> elseRule)
        {
            _elseRule = elseRule;
            return this;
        }

        /// <summary>
        /// Sestaví pravidlo podle nastavené konfigurace.
        /// </summary>
        /// <returns>Vytvořené pravidlo.</returns>
        public override Rule<TInput> Build()
        {
            if (_conditionFunc == null)
                throw new InvalidOperationException("Podmínková funkce musí být nastavena.");

            var rule = new ConditionalRule<TInput>(
                Id,
                Type,
                _conditionFunc,
                _thenRule,
                _elseRule,
                RuleId,
                Name ?? "Unnamed Conditional Rule",
                Description,
                Priority,
                IsActive,
                Timestamp);

            return rule;
        }
    }
}
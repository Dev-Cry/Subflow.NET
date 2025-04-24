// Ruleflow.NET/Engine/Validation/Conditions/RuleConditionBuilder.cs
using Ruleflow.NET.Engine.Validation.Builders;
using Ruleflow.NET.Engine.Validation.Core.Base;
using Ruleflow.NET.Engine.Validation.Enums;
using Ruleflow.NET.Engine.Validation.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ruleflow.NET.Engine.Validation.Conditions
{
    /// <summary>
    /// Implementace builderu pro podmíněná validační pravidla.
    /// </summary>
    /// <typeparam name="T">Typ validovaných dat</typeparam>
    internal class RuleConditionBuilder<T> : IRuleConditionBuilder<T>, IRuleConditionElseBuilder<T>, IRuleConditionThenBuilder<T>
    {
        private readonly ValidationRuleBuilder<T> _baseBuilder;
        private readonly Func<T, bool> _condition;
        private ValidationRuleBuilder<T>? _thenBuilder;
        private readonly List<(Func<T, bool> Condition, ValidationRuleBuilder<T> Builder)> _elseIfBuilders = [];
        private ValidationRuleBuilder<T>? _elseBuilder;

        /// <summary>
        /// Inicializuje novou instanci builderu pro podmíněná validační pravidla.
        /// </summary>
        /// <param name="baseBuilder">Základní builder</param>
        /// <param name="condition">Podmínka pro vyhodnocení</param>
        public RuleConditionBuilder(ValidationRuleBuilder<T> baseBuilder, Func<T, bool> condition)
        {
            _baseBuilder = baseBuilder ?? throw new ArgumentNullException(nameof(baseBuilder));
            _condition = condition ?? throw new ArgumentNullException(nameof(condition));
        }

        /// <inheritdoc />
        public IRuleConditionElseBuilder<T> Then(Action<ValidationRuleBuilder<T>> configureRule)
        {
            if (configureRule == null)
                throw new ArgumentNullException(nameof(configureRule));

            _thenBuilder = new ValidationRuleBuilder<T>();
            configureRule(_thenBuilder);
            return this;
        }

        /// <inheritdoc />
        public IRuleConditionThenBuilder<T> ElseIf(Func<T, bool> condition)
        {
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));

            var builder = new ValidationRuleBuilder<T>();
            _elseIfBuilders.Add((condition, builder));
            return this;
        }

        /// <inheritdoc />
        public IValidationRule<T> Else(Action<ValidationRuleBuilder<T>> configureRule)
        {
            if (configureRule == null)
                throw new ArgumentNullException(nameof(configureRule));

            _elseBuilder = new ValidationRuleBuilder<T>();
            configureRule(_elseBuilder);
            return Build();
        }

        /// <inheritdoc />
        public IValidationRule<T> Build()
        {
            // Pokud nebyl definován then blok, vyhodíme výjimku
            if (_thenBuilder == null)
                throw new InvalidOperationException("Musí být definován alespoň Then blok");

            // Pokud nebyl explicitně definován ID, vygenerujeme ho
            var baseRule = _baseBuilder.Build();

            // Připravíme pravidla pro jednotlivé větve
            var thenRule = _thenBuilder.Build();

            var elseIfRules = _elseIfBuilders
                .Select(x => (x.Condition, x.Builder.Build()))
                .ToList();

            var elseRule = _elseBuilder?.Build();

            return new ConditionalBranchRule<T>(
                baseRule,
                _condition,
                thenRule,
                elseIfRules,
                elseRule
            );
        }
    }

    /// <summary>
    /// Implementace validačního pravidla pro podmíněné větvení.
    /// </summary>
    /// <typeparam name="T">Typ validovaných dat</typeparam>
    internal class ConditionalBranchRule<T> : IdentifiableValidationRule<T>, IPrioritizedValidationRule<T>
    {
        private readonly IValidationRule<T> _baseRule;
        private readonly Func<T, bool> _condition;
        private readonly IValidationRule<T> _thenRule;
        private readonly List<(Func<T, bool> Condition, IValidationRule<T> Rule)> _elseIfRules;
        private readonly IValidationRule<T>? _elseRule;

        /// <summary>
        /// Inicializuje novou instanci validačního pravidla pro podmíněné větvení.
        /// </summary>
        /// <param name="baseRule">Základní pravidlo</param>
        /// <param name="condition">Podmínka pro vyhodnocení</param>
        /// <param name="thenRule">Pravidlo pro then větev</param>
        /// <param name="elseIfRules">Pravidla pro else-if větve</param>
        /// <param name="elseRule">Pravidlo pro else větev</param>
        public ConditionalBranchRule(
            IValidationRule<T> baseRule,
            Func<T, bool> condition,
            IValidationRule<T> thenRule,
            List<(Func<T, bool> Condition, IValidationRule<T> Rule)> elseIfRules,
            IValidationRule<T>? elseRule)
            : base(GetRuleId(baseRule))
        {
            _baseRule = baseRule ?? throw new ArgumentNullException(nameof(baseRule));
            _condition = condition ?? throw new ArgumentNullException(nameof(condition));
            _thenRule = thenRule ?? throw new ArgumentNullException(nameof(thenRule));
            _elseIfRules = elseIfRules ?? [];
            _elseRule = elseRule;
        }

        /// <inheritdoc />
        public override ValidationSeverity DefaultSeverity => _baseRule.DefaultSeverity;

        /// <inheritdoc />
        public override int Priority => _baseRule is IPrioritizedValidationRule<T> prioritized ? prioritized.Priority : 0;

        /// <inheritdoc />
        public override void Validate(T input)
        {
            if (_condition(input))
            {
                _thenRule.Validate(input);
            }
            else
            {
                foreach (var (condition, rule) in _elseIfRules)
                {
                    if (condition(input))
                    {
                        rule.Validate(input);
                        return;
                    }
                }

                _elseRule?.Validate(input);
            }
        }

        private static string GetRuleId(IValidationRule<T> rule)
        {
            return rule is IIdentifiableValidationRule<T> identifiable
                ? identifiable.RuleId
                : Guid.NewGuid().ToString();
        }
    }
}
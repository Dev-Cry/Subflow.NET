// Ruleflow.NET/Engine/Validation/Conditions/RuleSwitchBuilder.cs
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
    /// Implementace builderu pro validační pravidla založená na switch konstrukci.
    /// </summary>
    /// <typeparam name="T">Typ validovaných dat</typeparam>
    /// <typeparam name="TValue">Typ hodnoty pro switch</typeparam>
    internal class RuleSwitchBuilder<T, TValue> : IRuleSwitchBuilder<T, TValue>
    {
        private readonly ValidationRuleBuilder<T> _baseBuilder;
        private readonly Func<T, TValue> _valueSelector;
        private readonly Dictionary<TValue, ValidationRuleBuilder<T>> _caseBuilders = new();
        private ValidationRuleBuilder<T>? _defaultBuilder;

        /// <summary>
        /// Inicializuje novou instanci builderu pro switch validační pravidla.
        /// </summary>
        /// <param name="baseBuilder">Základní builder</param>
        /// <param name="valueSelector">Funkce pro získání hodnoty pro switch</param>
        public RuleSwitchBuilder(ValidationRuleBuilder<T> baseBuilder, Func<T, TValue> valueSelector)
        {
            _baseBuilder = baseBuilder ?? throw new ArgumentNullException(nameof(baseBuilder));
            _valueSelector = valueSelector ?? throw new ArgumentNullException(nameof(valueSelector));
        }

        /// <inheritdoc />
        public IRuleSwitchBuilder<T, TValue> Case(TValue value, Action<ValidationRuleBuilder<T>> configureRule)
        {
            if (configureRule == null)
                throw new ArgumentNullException(nameof(configureRule));

            var builder = new ValidationRuleBuilder<T>();
            configureRule(builder);
            _caseBuilders[value] = builder;
            return this;
        }

        /// <inheritdoc />
        public IRuleSwitchBuilder<T, TValue> Default(Action<ValidationRuleBuilder<T>> configureRule)
        {
            if (configureRule == null)
                throw new ArgumentNullException(nameof(configureRule));

            _defaultBuilder = new ValidationRuleBuilder<T>();
            configureRule(_defaultBuilder);
            return this;
        }

        /// <inheritdoc />
        public IValidationRule<T> Build()
        {
            // Pokud nebyl definován žádný case, vyhodíme výjimku
            if (_caseBuilders.Count == 0)
                throw new InvalidOperationException("Musí být definován alespoň jeden Case");

            // Připravíme pravidla pro jednotlivé case větve
            var caseRules = _caseBuilders.ToDictionary(
                x => x.Key,
                x => x.Value.Build()
            );

            var defaultRule = _defaultBuilder?.Build();

            return new SwitchRule<T, TValue>(
                _baseBuilder.Build(),
                _valueSelector,
                caseRules,
                defaultRule
            );
        }
    }

    /// <summary>
    /// Implementace validačního pravidla pro switch větvení.
    /// </summary>
    /// <typeparam name="T">Typ validovaných dat</typeparam>
    /// <typeparam name="TValue">Typ hodnoty pro switch</typeparam>
    internal class SwitchRule<T, TValue> : IdentifiableValidationRule<T>, IPrioritizedValidationRule<T>
    {
        private readonly IValidationRule<T> _baseRule;
        private readonly Func<T, TValue> _valueSelector;
        private readonly Dictionary<TValue, IValidationRule<T>> _caseRules;
        private readonly IValidationRule<T>? _defaultRule;

        /// <summary>
        /// Inicializuje novou instanci validačního pravidla pro switch větvení.
        /// </summary>
        /// <param name="baseRule">Základní pravidlo</param>
        /// <param name="valueSelector">Funkce pro získání hodnoty pro switch</param>
        /// <param name="caseRules">Pravidla pro jednotlivé case větve</param>
        /// <param name="defaultRule">Pravidlo pro default větev</param>
        public SwitchRule(
            IValidationRule<T> baseRule,
            Func<T, TValue> valueSelector,
            Dictionary<TValue, IValidationRule<T>> caseRules,
            IValidationRule<T>? defaultRule)
            : base(GetRuleId(baseRule))
        {
            _baseRule = baseRule ?? throw new ArgumentNullException(nameof(baseRule));
            _valueSelector = valueSelector ?? throw new ArgumentNullException(nameof(valueSelector));
            _caseRules = caseRules ?? throw new ArgumentNullException(nameof(caseRules));
            _defaultRule = defaultRule;
        }

        /// <inheritdoc />
        public override ValidationSeverity DefaultSeverity => _baseRule.DefaultSeverity;

        /// <inheritdoc />
        public override int Priority => _baseRule is IPrioritizedValidationRule<T> prioritized ? prioritized.Priority : 0;

        /// <inheritdoc />
        public override void Validate(T input)
        {
            var value = _valueSelector(input);

            if (_caseRules.TryGetValue(value, out var rule))
            {
                rule.Validate(input);
            }
            else
            {
                _defaultRule?.Validate(input);
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
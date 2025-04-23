// RuleflowExtensions.cs - Rozšiřující metody pro vytváření pravidel
using Ruleflow.NET.Engine.Validation.Builders;
using Ruleflow.NET.Engine.Validation.Core.Base;
using Ruleflow.NET.Engine.Validation.Core.Validators;
using Ruleflow.NET.Engine.Validation.Enums;
using Ruleflow.NET.Engine.Validation.Interfaces;
using System;
using System.Collections.Generic;

namespace Ruleflow.NET.Engine.Validation
{
    /// <summary>
    /// Poskytuje rozšiřující metody pro snadnější práci s validačním systémem.
    /// Obsahuje pomocné metody pro vytváření pravidel, validátorů a provádění validace.
    /// </summary>
    public static class RuleflowExtensions
    {
        /// <summary>
        /// Vytvoří nový builder pro standardní validační pravidlo.
        /// </summary>
        /// <typeparam name="T">Typ vstupních dat, která budou validována</typeparam>
        /// <returns>Builder pro konfiguraci pravidla</returns>
        public static ValidationRuleBuilder<T> CreateRule<T>()
        {
            return new ValidationRuleBuilder<T>();
        }

        /// <summary>
        /// Vytvoří nový builder pro validační pravidlo závisející na jiných pravidlech.
        /// </summary>
        /// <typeparam name="T">Typ vstupních dat, která budou validována</typeparam>
        /// <param name="ruleId">Identifikátor pravidla</param>
        /// <returns>Builder pro konfiguraci závislého pravidla</returns>
        public static DependentRuleBuilder<T> CreateDependentRule<T>(string ruleId)
        {
            return new DependentRuleBuilder<T>(ruleId);
        }

        /// <summary>
        /// Vytvoří validátor ze seznamu validačních pravidel.
        /// </summary>
        /// <typeparam name="T">Typ vstupních dat, která budou validována</typeparam>
        /// <param name="rules">Seznam pravidel pro validátor</param>
        /// <param name="logger">Logger pro zaznamenávání průběhu validace (volitelné)</param>
        /// <returns>Validátor připravený k použití</returns>
        public static IValidator<T> CreateValidator<T>(this IEnumerable<IValidationRule<T>> rules, Microsoft.Extensions.Logging.ILogger logger = null)
        {
            return new DependencyAwareValidator<T>(rules, logger as Microsoft.Extensions.Logging.ILogger<DependencyAwareValidator<T>>);
        }

        /// <summary>
        /// Provede validaci vstupní hodnoty přímo bez nutnosti vytváření validátoru.
        /// </summary>
        /// <typeparam name="T">Typ vstupní hodnoty</typeparam>
        /// <param name="input">Vstupní hodnota k validaci</param>
        /// <param name="rules">Seznam pravidel pro validaci</param>
        /// <param name="mode">Režim validace (výchozí: ReturnResult)</param>
        /// <returns>Výsledek validace</returns>
        /// <exception cref="AggregateException">Vyhozeno při ThrowOnError módu a selhání validace</exception>
        public static IValidationResult Validate<T>(this T input, IEnumerable<IValidationRule<T>> rules, ValidationMode mode = ValidationMode.ReturnResult)
        {
            var validator = rules.CreateValidator();
            var result = validator.ValidateWithResult(input);

            if (!result.IsValid && mode == ValidationMode.ThrowOnError)
                validator.Validate(input, mode);

            return result;
        }
    }

    /// <summary>
    /// Builder pro vytváření validačních pravidel se závislostmi na jiných pravidlech.
    /// Umožňuje intuitivní konstrukci závislých pravidel pomocí fluent API.
    /// </summary>
    /// <typeparam name="T">Typ vstupních dat, která budou validována</typeparam>
    public class DependentRuleBuilder<T>
    {
        private readonly string _ruleId;
        private Action<T> _validationAction;
        private string _errorMessage = "Validace selhala";
        private ValidationSeverity _severity = ValidationSeverity.Error;
        private List<string> _dependsOn = new List<string>();
        private DependencyType _dependencyType = DependencyType.RequiresAllSuccess;
        private int _priority = 0;

        /// <summary>
        /// Inicializuje novou instanci builderu pro závislé pravidlo.
        /// </summary>
        /// <param name="ruleId">Identifikátor pravidla</param>
        /// <exception cref="ArgumentNullException">Vyhozeno, pokud je ruleId null</exception>
        public DependentRuleBuilder(string ruleId)
        {
            _ruleId = ruleId ?? throw new ArgumentNullException(nameof(ruleId));
        }

        /// <summary>
        /// Definuje validační akci, která bude provedena při vyhodnocení pravidla.
        /// </summary>
        /// <param name="validationAction">Validační akce</param>
        /// <returns>Tento builder pro zřetězení volání</returns>
        /// <exception cref="ArgumentNullException">Vyhozeno, pokud je validationAction null</exception>
        public DependentRuleBuilder<T> WithAction(Action<T> validationAction)
        {
            _validationAction = validationAction ?? throw new ArgumentNullException(nameof(validationAction));
            return this;
        }

        /// <summary>
        /// Definuje chybovou zprávu, která bude použita při selhání validace.
        /// </summary>
        /// <param name="errorMessage">Chybová zpráva</param>
        /// <returns>Tento builder pro zřetězení volání</returns>
        /// <exception cref="ArgumentNullException">Vyhozeno, pokud je errorMessage null</exception>
        public DependentRuleBuilder<T> WithMessage(string errorMessage)
        {
            _errorMessage = errorMessage ?? throw new ArgumentNullException(nameof(errorMessage));
            return this;
        }

        /// <summary>
        /// Definuje závažnost chyby při selhání validace.
        /// </summary>
        /// <param name="severity">Závažnost chyby</param>
        /// <returns>Tento builder pro zřetězení volání</returns>
        public DependentRuleBuilder<T> WithSeverity(ValidationSeverity severity)
        {
            _severity = severity;
            return this;
        }

        /// <summary>
        /// Definuje ID pravidel, na kterých toto pravidlo závisí.
        /// </summary>
        /// <param name="ruleIds">Identifikátory pravidel</param>
        /// <returns>Tento builder pro zřetězení volání</returns>
        /// <exception cref="ArgumentException">Vyhozeno, pokud ruleIds je prázdné</exception>
        public DependentRuleBuilder<T> DependsOn(params string[] ruleIds)
        {
            if (ruleIds == null || ruleIds.Length == 0)
                throw new ArgumentException("Musí být definován alespoň jeden závislý identifikátor pravidla", nameof(ruleIds));

            _dependsOn.AddRange(ruleIds);
            return this;
        }

        /// <summary>
        /// Definuje typ závislosti, který určuje, kdy se pravidlo má spustit.
        /// </summary>
        /// <param name="dependencyType">Typ závislosti</param>
        /// <returns>Tento builder pro zřetězení volání</returns>
        public DependentRuleBuilder<T> WithDependencyType(DependencyType dependencyType)
        {
            _dependencyType = dependencyType;
            return this;
        }

        /// <summary>
        /// Definuje prioritu pravidla, která určuje pořadí vyhodnocení.
        /// Pravidla s vyšší prioritou jsou vyhodnocena dříve.
        /// </summary>
        /// <param name="priority">Hodnota priority</param>
        /// <returns>Tento builder pro zřetězení volání</returns>
        public DependentRuleBuilder<T> WithPriority(int priority)
        {
            _priority = priority;
            return this;
        }

        /// <summary>
        /// Vytvoří závislé validační pravidlo podle nastavených parametrů.
        /// </summary>
        /// <returns>Implementace IDependentValidationRule&lt;T&gt;</returns>
        /// <exception cref="InvalidOperationException">Vyhozeno, pokud nebyla definována validační akce nebo závislosti</exception>
        public IDependentValidationRule<T> Build()
        {
            if (_validationAction == null)
                throw new InvalidOperationException("Validační akce nebyla definována");

            if (_dependsOn.Count == 0)
                throw new InvalidOperationException("Musí být definován alespoň jeden závislý identifikátor pravidla");

            return new DynamicDependentRule<T>(_ruleId, _validationAction, _errorMessage, _severity, _dependsOn, _dependencyType, _priority);
        }

        /// <summary>
        /// Vnitřní implementace závislého validačního pravidla.
        /// </summary>
        /// <typeparam name="TInput">Typ vstupních dat</typeparam>
        private class DynamicDependentRule<TInput> : DependentValidationRule<TInput>, IPrioritizedValidationRule<TInput>
        {
            private readonly Action<TInput> _validationAction;
            private readonly string _errorMessage;
            private readonly ValidationSeverity _severity;
            private readonly int _rulePriority;

            /// <summary>
            /// Inicializuje novou instanci závislého validačního pravidla.
            /// </summary>
            public DynamicDependentRule(string ruleId, Action<TInput> validationAction,
                string errorMessage, ValidationSeverity severity,
                IEnumerable<string> dependsOn, DependencyType dependencyType, int priority)
                : base(ruleId, dependsOn, dependencyType)
            {
                _validationAction = validationAction;
                _errorMessage = errorMessage;
                _severity = severity;
                _rulePriority = priority;
            }

            // Implementace vlastností z rozhraní
            public override ValidationSeverity DefaultSeverity => _severity;
            public override int Priority => _rulePriority;

            /// <summary>
            /// Provede validaci vstupních dat.
            /// </summary>
            /// <param name="input">Vstupní data k validaci</param>
            /// <exception cref="ArgumentException">Vyhozeno s chybovou zprávou, pokud validace selže</exception>
            public override void Validate(TInput input)
            {
                try
                {
                    // Provedení validační akce
                    _validationAction(input);
                }
                catch (Exception ex) when (!(ex is ArgumentException))
                {
                    // Převedení výjimky na ArgumentException s nastavenou zprávou
                    throw new ArgumentException(_errorMessage, ex);
                }
            }
        }
    }
}
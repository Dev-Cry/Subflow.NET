// ValidationRuleBuilder.cs - Definuje fluent API pro tvorbu pravidel
using Ruleflow.NET.Engine.Validation.Core.Base;
using Ruleflow.NET.Engine.Validation.Enums;
using Ruleflow.NET.Engine.Validation.Interfaces;
using System;
using System.Linq.Expressions;

namespace Ruleflow.NET.Engine.Validation.Builders
{
    /// <summary>
    /// Builder pro vytváření validačních pravidel pomocí fluent API.
    /// Umožňuje intuitivní konstrukci pravidel bez nutnosti dědění z abstraktních tříd.
    /// </summary>
    /// <typeparam name="T">Typ vstupních dat, která budou validována</typeparam>
    public class ValidationRuleBuilder<T>
    {
        // Akce, která provádí vlastní validaci
        private Action<T> _validationAction;

        // Chybová zpráva, která bude použita, pokud validace selže
        private string _errorMessage = "Validace selhala";

        // Závažnost chyby při selhání validace
        private ValidationSeverity _severity = ValidationSeverity.Error;

        // Identifikátor pravidla, používá se pro závislosti mezi pravidly
        private string _ruleId;

        // Podmínka, která určuje, zda se má pravidlo vyhodnotit (nepovinné)
        private Func<T, bool> _condition;

        // Priorita pravidla ovlivňující pořadí vyhodnocení (vyšší číslo = dřívější vyhodnocení)
        private int _priority = 0;

        /// <summary>
        /// Definuje validační akci, která bude provedena při vyhodnocení pravidla.
        /// Akce by měla vyhodit výjimku, pokud validace selže.
        /// </summary>
        /// <param name="validationAction">Validační akce</param>
        /// <returns>Tento builder pro zřetězení volání</returns>
        public ValidationRuleBuilder<T> WithAction(Action<T> validationAction)
        {
            _validationAction = validationAction ?? throw new ArgumentNullException(nameof(validationAction));
            return this;
        }

        /// <summary>
        /// Definuje chybovou zprávu, která bude použita při selhání validace.
        /// </summary>
        /// <param name="errorMessage">Chybová zpráva</param>
        /// <returns>Tento builder pro zřetězení volání</returns>
        public ValidationRuleBuilder<T> WithMessage(string errorMessage)
        {
            _errorMessage = errorMessage ?? throw new ArgumentNullException(nameof(errorMessage));
            return this;
        }

        /// <summary>
        /// Definuje závažnost chyby při selhání validace.
        /// </summary>
        /// <param name="severity">Závažnost chyby</param>
        /// <returns>Tento builder pro zřetězení volání</returns>
        public ValidationRuleBuilder<T> WithSeverity(ValidationSeverity severity)
        {
            _severity = severity;
            return this;
        }

        /// <summary>
        /// Definuje jednoznačný identifikátor pravidla.
        /// Pokud není nastaven, bude vygenerován náhodně při volání Build().
        /// </summary>
        /// <param name="ruleId">Identifikátor pravidla</param>
        /// <returns>Tento builder pro zřetězení volání</returns>
        public ValidationRuleBuilder<T> WithId(string ruleId)
        {
            _ruleId = ruleId ?? throw new ArgumentNullException(nameof(ruleId));
            return this;
        }

        /// <summary>
        /// Definuje podmínku, za které se pravidlo vyhodnotí.
        /// Pokud je podmínka nastavena, pravidlo se vyhodnotí pouze když podmínka vrátí true.
        /// </summary>
        /// <param name="condition">Funkce vracející boolean, určující zda se má pravidlo vyhodnotit</param>
        /// <returns>Tento builder pro zřetězení volání</returns>
        public ValidationRuleBuilder<T> WithCondition(Func<T, bool> condition)
        {
            _condition = condition ?? throw new ArgumentNullException(nameof(condition));
            return this;
        }

        /// <summary>
        /// Definuje prioritu pravidla, která určuje pořadí vyhodnocení.
        /// Pravidla s vyšší prioritou jsou vyhodnocena dříve.
        /// </summary>
        /// <param name="priority">Hodnota priority</param>
        /// <returns>Tento builder pro zřetězení volání</returns>
        public ValidationRuleBuilder<T> WithPriority(int priority)
        {
            _priority = priority;
            return this;
        }

        /// <summary>
        /// Vytvoří validační pravidlo podle nastavených parametrů.
        /// Pokud je nastavena podmínka, vytvoří podmíněné pravidlo, jinak standardní.
        /// </summary>
        /// <returns>Implementace IValidationRule&lt;T&gt;</returns>
        /// <exception cref="InvalidOperationException">Vyhozeno, pokud nebyla definována validační akce</exception>
        public IValidationRule<T> Build()
        {
            if (_validationAction == null)
                throw new InvalidOperationException("Validační akce nebyla definována");

            // Pokud není ID explicitně nastaveno, vygenerujeme náhodné
            if (string.IsNullOrEmpty(_ruleId))
                _ruleId = Guid.NewGuid().ToString();

            // Podle toho, zda je nastavena podmínka, vracíme vhodný typ pravidla
            if (_condition != null)
                return new DynamicConditionalRule<T>(_ruleId, _validationAction, _errorMessage, _severity, _condition, _priority);
            else
                return new DynamicRule<T>(_ruleId, _validationAction, _errorMessage, _severity, _priority);
        }

        /// <summary>
        /// Vnitřní implementace standardního validačního pravidla.
        /// </summary>
        /// <typeparam name="TInput">Typ vstupních dat</typeparam>
        private class DynamicRule<TInput> : IdentifiableValidationRule<TInput>, IPrioritizedValidationRule<TInput>
        {
            private readonly Action<TInput> _validationAction;
            private readonly string _errorMessage;
            private readonly ValidationSeverity _severity;
            private readonly int _rulePriority;

            public DynamicRule(string ruleId, Action<TInput> validationAction, string errorMessage,
                ValidationSeverity severity, int priority) : base(ruleId)
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

        /// <summary>
        /// Vnitřní implementace podmíněného validačního pravidla.
        /// </summary>
        /// <typeparam name="TInput">Typ vstupních dat</typeparam>
        private class DynamicConditionalRule<TInput> : ConditionalValidationRule<TInput>, IPrioritizedValidationRule<TInput>
        {
            private readonly Action<TInput> _validationAction;
            private readonly string _errorMessage;
            private readonly ValidationSeverity _severity;
            private readonly Func<TInput, bool> _condition;
            private readonly int _rulePriority;

            public DynamicConditionalRule(string ruleId, Action<TInput> validationAction,
                string errorMessage, ValidationSeverity severity,
                Func<TInput, bool> condition, int priority) : base(ruleId)
            {
                _validationAction = validationAction;
                _errorMessage = errorMessage;
                _severity = severity;
                _condition = condition;
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

            /// <summary>
            /// Určuje, zda by se pravidlo mělo vyhodnotit pro daný vstup a kontext.
            /// </summary>
            /// <param name="input">Vstupní data</param>
            /// <param name="context">Validační kontext</param>
            /// <returns>True, pokud by pravidlo mělo být vyhodnoceno; jinak false</returns>
            public override bool ShouldValidate(TInput input, Core.Context.ValidationContext context)
            {
                // Vyhodnocení podmínky
                return _condition(input);
            }
        }
    }
}
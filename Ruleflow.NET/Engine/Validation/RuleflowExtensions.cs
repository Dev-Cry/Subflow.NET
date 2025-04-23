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
    public static class RuleflowExtensions
    {
        // Metoda pro vytvoření nového pravidla
        public static ValidationRuleBuilder<T> CreateRule<T>()
        {
            return new ValidationRuleBuilder<T>();
        }

        // Metoda pro vytvoření dependentního pravidla
        public static DependentRuleBuilder<T> CreateDependentRule<T>(string ruleId)
        {
            return new DependentRuleBuilder<T>(ruleId);
        }

        // Metoda pro vytvoření validátoru z kolekce pravidel
        public static IValidator<T> CreateValidator<T>(this IEnumerable<IValidationRule<T>> rules, Microsoft.Extensions.Logging.ILogger logger = null)
        {
            return new DependencyAwareValidator<T>(rules, logger as Microsoft.Extensions.Logging.ILogger<DependencyAwareValidator<T>>);
        }

        // Metoda pro snadnou validaci bez tvorby validátoru
        public static IValidationResult Validate<T>(this T input, IEnumerable<IValidationRule<T>> rules, ValidationMode mode = ValidationMode.ReturnResult)
        {
            var validator = rules.CreateValidator();
            var result = validator.ValidateWithResult(input);

            if (!result.IsValid && mode == ValidationMode.ThrowOnError)
                validator.Validate(input, mode);

            return result;
        }
    }

    // Nový builder pro tvorbu závislých pravidel
    public class DependentRuleBuilder<T>
    {
        private readonly string _ruleId;
        private Action<T> _validationAction;
        private string _errorMessage = "Validace selhala";
        private ValidationSeverity _severity = ValidationSeverity.Error;
        private List<string> _dependsOn = new List<string>();
        private DependencyType _dependencyType = DependencyType.RequiresAllSuccess;
        private int _priority = 0;

        public DependentRuleBuilder(string ruleId)
        {
            _ruleId = ruleId ?? throw new ArgumentNullException(nameof(ruleId));
        }

        public DependentRuleBuilder<T> WithAction(Action<T> validationAction)
        {
            _validationAction = validationAction ?? throw new ArgumentNullException(nameof(validationAction));
            return this;
        }

        public DependentRuleBuilder<T> WithMessage(string errorMessage)
        {
            _errorMessage = errorMessage ?? throw new ArgumentNullException(nameof(errorMessage));
            return this;
        }

        public DependentRuleBuilder<T> WithSeverity(ValidationSeverity severity)
        {
            _severity = severity;
            return this;
        }

        public DependentRuleBuilder<T> DependsOn(params string[] ruleIds)
        {
            if (ruleIds == null || ruleIds.Length == 0)
                throw new ArgumentException("Musí být definován alespoň jeden závislý identifikátor pravidla", nameof(ruleIds));

            _dependsOn.AddRange(ruleIds);
            return this;
        }

        public DependentRuleBuilder<T> WithDependencyType(DependencyType dependencyType)
        {
            _dependencyType = dependencyType;
            return this;
        }

        public DependentRuleBuilder<T> WithPriority(int priority)
        {
            _priority = priority;
            return this;
        }

        public IDependentValidationRule<T> Build()
        {
            if (_validationAction == null)
                throw new InvalidOperationException("Validační akce nebyla definována");

            if (_dependsOn.Count == 0)
                throw new InvalidOperationException("Musí být definován alespoň jeden závislý identifikátor pravidla");

            return new DynamicDependentRule<T>(_ruleId, _validationAction, _errorMessage, _severity, _dependsOn, _dependencyType, _priority);
        }

        private class DynamicDependentRule<TInput> : DependentValidationRule<TInput>, IPrioritizedValidationRule<TInput>
        {
            private readonly Action<TInput> _validationAction;
            private readonly string _errorMessage;
            private readonly ValidationSeverity _severity;
            private readonly int _rulePriority;

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

            public override ValidationSeverity DefaultSeverity => _severity;
            public override int Priority => _rulePriority;

            public override void Validate(TInput input)
            {
                try
                {
                    _validationAction(input);
                }
                catch (Exception ex) when (!(ex is ArgumentException))
                {
                    throw new ArgumentException(_errorMessage, ex);
                }
            }
        }
    }
}
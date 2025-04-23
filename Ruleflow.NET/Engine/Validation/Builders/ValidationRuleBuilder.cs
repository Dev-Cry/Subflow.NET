// ValidationRuleBuilder.cs - Definuje fluent API pro tvorbu pravidel
using Ruleflow.NET.Engine.Validation.Core.Base;
using Ruleflow.NET.Engine.Validation.Enums;
using Ruleflow.NET.Engine.Validation.Interfaces;
using System;
using System.Linq.Expressions;

namespace Ruleflow.NET.Engine.Validation.Builders
{
    public class ValidationRuleBuilder<T>
    {
        private Action<T> _validationAction;
        private string _errorMessage = "Validace selhala";
        private ValidationSeverity _severity = ValidationSeverity.Error;
        private string _ruleId;
        private Func<T, bool> _condition;
        private int _priority = 0;

        public ValidationRuleBuilder<T> WithAction(Action<T> validationAction)
        {
            _validationAction = validationAction ?? throw new ArgumentNullException(nameof(validationAction));
            return this;
        }

        public ValidationRuleBuilder<T> WithMessage(string errorMessage)
        {
            _errorMessage = errorMessage ?? throw new ArgumentNullException(nameof(errorMessage));
            return this;
        }

        public ValidationRuleBuilder<T> WithSeverity(ValidationSeverity severity)
        {
            _severity = severity;
            return this;
        }

        public ValidationRuleBuilder<T> WithId(string ruleId)
        {
            _ruleId = ruleId ?? throw new ArgumentNullException(nameof(ruleId));
            return this;
        }

        public ValidationRuleBuilder<T> WithCondition(Func<T, bool> condition)
        {
            _condition = condition ?? throw new ArgumentNullException(nameof(condition));
            return this;
        }

        public ValidationRuleBuilder<T> WithPriority(int priority)
        {
            _priority = priority;
            return this;
        }

        // Vytváření pravidla: standardní pravidlo nebo podmíněné
        public IValidationRule<T> Build()
        {
            if (_validationAction == null)
                throw new InvalidOperationException("Validační akce nebyla definována");

            if (string.IsNullOrEmpty(_ruleId))
                _ruleId = Guid.NewGuid().ToString();

            if (_condition != null)
                return new DynamicConditionalRule<T>(_ruleId, _validationAction, _errorMessage, _severity, _condition, _priority);
            else
                return new DynamicRule<T>(_ruleId, _validationAction, _errorMessage, _severity, _priority);
        }

        // Pomocné třídy pro dynamická pravidla
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

            public override bool ShouldValidate(TInput input, Core.Context.ValidationContext context)
            {
                return _condition(input);
            }
        }
    }
}
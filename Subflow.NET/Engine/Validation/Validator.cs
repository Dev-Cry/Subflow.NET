using Microsoft.Extensions.Logging;
using Subflow.NET.Engine.Validation.Enums;
using Subflow.NET.Engine.Validation.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Subflow.NET.Engine.Validation
{
    public class Validator<T> : IValidator<T>
    {
        private readonly IEnumerable<IValidationRule<T>> _rules;
        private readonly ILogger? _logger;

        public Validator(IEnumerable<IValidationRule<T>> rules, ILogger? logger = null)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            _logger = logger;
        }

        public void Validate(T input, ValidationMode mode = ValidationMode.ThrowOnError)
        {
            var result = ValidateWithResult(input);
            if (!result.IsValid && mode == ValidationMode.ThrowOnError)
                throw new AggregateException(result.Errors.Select(e => new Exception(e)));
        }

        public IValidationResult ValidateWithResult(T input)
        {
            var result = new ValidationResult();

            foreach (var rule in _rules)
            {
                try
                {
                    rule.Validate(input);
                    _logger?.LogDebug("Rule {RuleName} passed.", rule.GetType().Name);
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Rule {RuleName} failed: {Message}", rule.GetType().Name, ex.Message);
                    result.AddError(ex.Message);
                }
            }

            return result;
        }
    }
}

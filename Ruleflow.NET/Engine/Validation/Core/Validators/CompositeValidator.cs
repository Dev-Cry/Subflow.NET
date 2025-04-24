using Microsoft.Extensions.Logging;
using Ruleflow.NET.Engine.Validation.Core.Results;
using Ruleflow.NET.Engine.Validation.Enums;
using Ruleflow.NET.Engine.Validation.Interfaces;
using System;
using System.Collections.Generic;

namespace Ruleflow.NET.Engine.Validation.Core.Validators
{
    public class CompositeValidator<T> : ICompositeValidator<T>
    {
        private readonly IEnumerable<IValidator<T>> _validators;
        private readonly ILogger? _logger;

        public CompositeValidator(IEnumerable<IValidator<T>> validators, ILogger? logger = null)
        {
            _validators = validators ?? throw new ArgumentNullException(nameof(validators));
            _logger = logger;
        }

        public IValidationResult CollectValidationResults(T input)
        {
            var combinedResult = new ValidationResult();

            foreach (var validator in _validators)
            {
                var result = validator.CollectValidationResults(input);
                combinedResult.AddErrors(result.Errors);
            }

            _logger?.LogInformation("Composite validation result: {ErrorCount} errors.", combinedResult.Errors.Count);
            return combinedResult;
        }

        public void ValidateOrThrow(T input)
        {
            var result = CollectValidationResults(input);
            result.ThrowIfInvalid();
        }

        public void Validate(T input, ValidationMode mode = ValidationMode.ThrowOnError)
        {
            if (mode == ValidationMode.ThrowOnError)
            {
                ValidateOrThrow(input);
            }
            else
            {
                CollectValidationResults(input);
            }
        }

        // Implementace původní metody
        public IValidationResult ValidateWithResult(T input)
        {
            return CollectValidationResults(input);
        }
    }
}
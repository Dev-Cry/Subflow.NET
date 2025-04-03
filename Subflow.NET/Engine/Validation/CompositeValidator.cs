using Microsoft.Extensions.Logging;
using Ruleflow.NET.Engine.Validation.Enums;
using Ruleflow.NET.Engine.Validation.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ruleflow.NET.Engine.Validation
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

        public void Validate(T input, ValidationMode mode = ValidationMode.ThrowOnError)
        {
            foreach (var validator in _validators)
            {
                validator.Validate(input, mode);
            }
        }

        public IValidationResult ValidateWithResult(T input)
        {
            var combinedResult = new ValidationResult();

            foreach (var validator in _validators)
            {
                var result = validator.ValidateWithResult(input);
                combinedResult.AddErrors(result.Errors);
            }

            _logger?.LogInformation("Composite validation result: {ErrorCount} errors.", combinedResult.Errors.Count);
            return combinedResult;
        }
    }
}

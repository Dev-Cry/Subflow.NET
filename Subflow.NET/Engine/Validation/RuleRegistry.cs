using Microsoft.Extensions.Logging;
using Subflow.NET.Engine.Validation.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Subflow.NET.Engine.Validation
{
    public class RuleRegistry : IRuleRegistry
    {
        private readonly IEnumerable<IValidationRule> _rules;
        private readonly ILoggerFactory? _loggerFactory;

        public RuleRegistry(IEnumerable<IValidationRule> rules, ILoggerFactory? loggerFactory = null)
        {
            _rules = rules;
            _loggerFactory = loggerFactory;
        }

        public IValidator<T> CreateValidator<T>()
        {
            var matchingRules = _rules
                .OfType<IValidationRule<T>>()
                .ToList();

            var logger = _loggerFactory?.CreateLogger<DependencyAwareValidator<T>>();
            return new DependencyAwareValidator<T>(matchingRules, logger);
        }
    }
}
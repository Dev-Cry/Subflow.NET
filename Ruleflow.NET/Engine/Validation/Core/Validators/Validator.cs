using Microsoft.Extensions.Logging;
using Ruleflow.NET.Engine.Validation.Core.Exceptions;
using Ruleflow.NET.Engine.Validation.Core.Results;
using Ruleflow.NET.Engine.Validation.Enums;
using Ruleflow.NET.Engine.Validation.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ruleflow.NET.Engine.Validation.Core.Validators
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

        /// <summary>
        /// Validuje vstupní data a vrátí objekt s výsledky validace bez vyhození výjimky.
        /// </summary>
        public IValidationResult CollectValidationResults(T input)
        {
            var result = new ValidationResult();

            foreach (var rule in _rules)
            {
                try
                {
                    rule.Validate(input);
                    _logger?.LogDebug("Pravidlo {RuleName} úspěšně prošlo validací.", rule.GetType().Name);
                }
                catch (Exception ex)
                {
                    var severity = rule.DefaultSeverity;

                    // Logování podle závažnosti pravidla
                    LogByLevel(_logger, severity, ex, "Pravidlo {RuleName} selhalo: {Message}",
                        rule.GetType().Name, ex.Message);

                    result.AddError(ex.Message, severity, rule.GetType().Name, ex);
                }
            }

            return result;
        }

        /// <summary>
        /// Validuje vstupní data a vyhodí výjimku při selhání validace.
        /// </summary>
        public void ValidateOrThrow(T input)
        {
            var result = CollectValidationResults(input);
            result.ThrowIfInvalid();
        }

        /// <summary>
        /// Validuje vstup podle zadaného režimu.
        /// Pro jasnější záměr použijte místo této metody ValidateOrThrow nebo CollectValidationResults.
        /// </summary>
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

        private static void LogByLevel(ILogger? logger, ValidationSeverity severity, Exception? ex, string message, params object[] args)
        {
            if (logger == null) return;

            switch (severity)
            {
                case ValidationSeverity.Verbose:
                    logger.LogTrace(ex, message, args);
                    break;
                case ValidationSeverity.Debug:
                    logger.LogDebug(ex, message, args);
                    break;
                case ValidationSeverity.Information:
                    logger.LogInformation(ex, message, args);
                    break;
                case ValidationSeverity.Warning:
                    logger.LogWarning(ex, message, args);
                    break;
                case ValidationSeverity.Error:
                    logger.LogError(ex, message, args);
                    break;
                case ValidationSeverity.Critical:
                    logger.LogCritical(ex, message, args);
                    break;
                default:
                    logger.LogError(ex, message, args);
                    break;
            }
        }
    }
}
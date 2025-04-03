using Microsoft.Extensions.Logging;
using Ruleflow.NET.Engine.Validation.Enums;
using Ruleflow.NET.Engine.Validation.Interfaces;
using Ruleflow.NET.Engine.Validation;
using System.ComponentModel.DataAnnotations;

namespace Ruleflow.NET.Engine.Validation
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
            {
                var criticalErrors = result.GetErrorsBySeverity(ValidationSeverity.Critical).ToList();
                if (criticalErrors.Any())
                {
                    throw new AggregateException("Validace selhala s kritickými chybami",
                        criticalErrors.Select(e => new ValidationException(e.Message, e)));
                }

                var errors = result.Errors.Where(e => e.Severity >= ValidationSeverity.Error).ToList();
                if (errors.Any())
                {
                    throw new AggregateException("Validace selhala",
                        errors.Select(e => new ValidationException(e.Message, e)));
                }
            }
        }

        public IValidationResult ValidateWithResult(T input)
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
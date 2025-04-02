using Microsoft.Extensions.Logging;
using Subflow.NET.Engine.Validation.Enums;
using Subflow.NET.Engine.Validation.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Subflow.NET.Engine.Validation
{
    /// <summary>
    /// Validátor podporující závislosti mezi pravidly a kontextovou validaci
    /// </summary>
    public class DependencyAwareValidator<T> : IValidator<T>, IContextAwareValidator<T>
    {
        private readonly IEnumerable<IValidationRule<T>> _rules;
        private readonly ILogger<DependencyAwareValidator<T>>? _logger;

        public DependencyAwareValidator(IEnumerable<IValidationRule<T>> rules, ILogger<DependencyAwareValidator<T>>? logger = null)
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
            var context = new ValidationContext();
            return ValidateWithContext(input, context);
        }

        public IValidationResult ValidateWithContext(T input, ValidationContext context)
        {
            var result = new ValidationResult();

            // Seřazení pravidel: nejprve podle priority, poté podle závislostí
            var prioritizedRules = _rules
                .OfType<IPrioritizedValidationRule<T>>()
                .OrderByDescending(r => r.Priority)
                .Cast<IValidationRule<T>>();

            var nonPrioritizedRules = _rules.Except(prioritizedRules);
            var orderedRules = prioritizedRules.Concat(nonPrioritizedRules).ToList();

            // Rozdělení na nezávislá a závislá pravidla
            var independentRules = orderedRules
                .Where(r => !(r is IDependentValidationRule<T>))
                .ToList();

            var dependentRules = orderedRules
                .OfType<IDependentValidationRule<T>>()
                .ToList();

            // Nejprve zpracuj nezávislá pravidla
            ProcessRules(independentRules, input, result, context);

            // Zpracuj závislá pravidla, dokud existují nějaká, která lze spustit
            var remainingRules = new List<IDependentValidationRule<T>>(dependentRules);
            int previousCount;

            do
            {
                previousCount = remainingRules.Count;

                var rulesToProcess = remainingRules
                    .Where(rule => ShouldProcessRule(rule, context))
                    .ToList();

                foreach (var rule in rulesToProcess)
                {
                    ProcessRule(rule, input, result, context);
                    remainingRules.Remove(rule);
                }

            } while (remainingRules.Count > 0 && remainingRules.Count < previousCount);

            // Přidání informace o neprocesovaných pravidlech
            foreach (var rule in remainingRules)
            {
                _logger?.LogWarning(
                    "Pravidlo {RuleId} nebylo vyhodnoceno, protože jeho závislosti nebyly splněny",
                    (rule as IIdentifiableValidationRule<T>)?.RuleId ?? rule.GetType().Name);
            }

            return result;
        }

        private bool ShouldProcessRule(IDependentValidationRule<T> rule, ValidationContext context)
        {
            var dependsOn = rule.DependsOn;
            if (!dependsOn.Any()) return true;

            switch (rule.DependencyType)
            {
                case DependencyType.RequiresAllSuccess:
                    return context.AllRulesSucceeded(dependsOn);

                case DependencyType.RequiresAnySuccess:
                    return context.AnyRuleSucceeded(dependsOn);

                case DependencyType.RequiresAllFailure:
                    return context.AllRulesFailed(dependsOn);

                case DependencyType.RequiresAnyFailure:
                    return context.AnyRuleFailed(dependsOn);

                default:
                    return false;
            }
        }

        private void ProcessRules(IEnumerable<IValidationRule<T>> rules, T input, ValidationResult result, ValidationContext context)
        {
            foreach (var rule in rules)
            {
                // Přeskočit podmíněná pravidla, která nesplňují podmínku
                if (rule is IConditionalValidationRule<T> conditionalRule &&
                    !conditionalRule.ShouldValidate(input, context))
                {
                    _logger?.LogDebug("Pravidlo {RuleName} přeskočeno, podmínka nebyla splněna",
                        rule.GetType().Name);
                    continue;
                }

                ProcessRule(rule, input, result, context);
            }
        }

        private void ProcessRule(IValidationRule<T> rule, T input, ValidationResult result, ValidationContext context)
        {
            try
            {
                rule.Validate(input);

                _logger?.LogDebug("Pravidlo {RuleName} úspěšně prošlo validací", rule.GetType().Name);

                if (rule is IIdentifiableValidationRule<T> identifiable)
                {
                    context.AddRuleResult(new ValidationRuleResult(identifiable.RuleId, true));
                }
            }
            catch (Exception ex)
            {
                var severity = rule.DefaultSeverity;

                // Logování podle závažnosti pravidla
                LogByLevel(_logger, severity, ex, "Pravidlo {RuleName} selhalo: {Message}",
                    rule.GetType().Name, ex.Message);

                var error = new ValidationError(ex.Message, severity,
                    rule.GetType().Name, ex);

                result.AddError(error);

                if (rule is IIdentifiableValidationRule<T> identifiable)
                {
                    context.AddRuleResult(new ValidationRuleResult(identifiable.RuleId, false, error));
                }
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
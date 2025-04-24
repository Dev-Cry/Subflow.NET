// Engine/Validation/Core/Validators/Execution/RuleExecutor.cs
using Microsoft.Extensions.Logging;
using Ruleflow.NET.Engine.Validation.Core.Context;
using Ruleflow.NET.Engine.Validation.Core.Results;
using Ruleflow.NET.Engine.Validation.Core.Validators.Dependency;
using Ruleflow.NET.Engine.Validation.Enums;
using Ruleflow.NET.Engine.Validation.Interfaces;
using System;
using System.Collections.Generic;

namespace Ruleflow.NET.Engine.Validation.Core.Validators.Execution
{
    /// <summary>
    /// Třída odpovědná za vykonávání validačních pravidel a sběr výsledků.
    /// </summary>
    /// <typeparam name="T">Typ validovaných dat</typeparam>
    internal class RuleExecutor<T>
    {
        private readonly ILogger<DependencyAwareValidator<T>>? _logger;
        private readonly RuleDependencyEvaluator _dependencyEvaluator;

        /// <summary>
        /// Inicializuje novou instanci vykonavatele pravidel.
        /// </summary>
        /// <param name="logger">Logger pro zaznamenávání průběhu validace</param>
        public RuleExecutor(ILogger<DependencyAwareValidator<T>>? logger = null)
        {
            _logger = logger;
            _dependencyEvaluator = new RuleDependencyEvaluator();
        }

        /// <summary>
        /// Vykoná seznam nezávislých pravidel a aktualizuje validační výsledek a kontext.
        /// </summary>
        /// <param name="rules">Seznam pravidel k vykonání</param>
        /// <param name="input">Vstupní data k validaci</param>
        /// <param name="result">Validační výsledek, do kterého budou přidány chyby</param>
        /// <param name="context">Validační kontext pro sdílení stavu mezi pravidly</param>
        public void ExecuteRules(
            IEnumerable<IValidationRule<T>> rules,
            T input,
            ValidationResult result,
            ValidationContext context)
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

                ExecuteRule(rule, input, result, context);
            }
        }

        /// <summary>
        /// Vykoná seznam závislých pravidel a aktualizuje validační výsledek a kontext.
        /// </summary>
        /// <param name="rules">Seznam závislých pravidel k vykonání</param>
        /// <param name="input">Vstupní data k validaci</param>
        /// <param name="result">Validační výsledek, do kterého budou přidány chyby</param>
        /// <param name="context">Validační kontext pro sdílení stavu mezi pravidly</param>
        public void ExecuteDependentRules(
            IEnumerable<IDependentValidationRule<T>> rules,
            T input,
            ValidationResult result,
            ValidationContext context)
        {
            foreach (var rule in rules)
            {
                // Podmíněné vyhodnocení závislostí
                if (_dependencyEvaluator.ShouldProcessRule(rule, context))
                {
                    // Přeskočit podmíněná pravidla, která nesplňují podmínku
                    if (rule is IConditionalValidationRule<T> conditionalRule &&
                        !conditionalRule.ShouldValidate(input, context))
                    {
                        _logger?.LogDebug("Pravidlo {RuleName} přeskočeno, podmínka nebyla splněna",
                            rule.GetType().Name);
                        continue;
                    }

                    ExecuteRule(rule, input, result, context);
                }
                else
                {
                    _logger?.LogDebug(
                        "Pravidlo {RuleName} přeskočeno, závislosti nebyly splněny",
                        rule.GetType().Name);
                }
            }
        }

        /// <summary>
        /// Vykoná jedno pravidlo a aktualizuje validační výsledek a kontext.
        /// </summary>
        /// <param name="rule">Pravidlo k vykonání</param>
        /// <param name="input">Vstupní data k validaci</param>
        /// <param name="result">Validační výsledek, do kterého budou přidány chyby</param>
        /// <param name="context">Validační kontext pro sdílení stavu mezi pravidly</param>
        private void ExecuteRule(
            IValidationRule<T> rule,
            T input,
            ValidationResult result,
            ValidationContext context)
        {
            try
            {
                // Zrušení validace, pokud byl požadován
                context.CancellationToken.ThrowIfCancellationRequested();

                rule.Validate(input);

                _logger?.LogDebug("Pravidlo {RuleName} úspěšně prošlo validací", rule.GetType().Name);

                if (rule is IIdentifiableValidationRule<T> identifiable)
                {
                    context.AddRuleResult(new ValidationRuleResult(identifiable.RuleId, true));
                }
            }
            catch (OperationCanceledException)
            {
                // Propagujeme zrušení operace
                throw;
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

        /// <summary>
        /// Loguje zprávu podle úrovně závažnosti.
        /// </summary>
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
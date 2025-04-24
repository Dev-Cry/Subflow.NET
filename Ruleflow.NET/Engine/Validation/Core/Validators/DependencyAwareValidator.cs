// Engine/Validation/Core/Validators/DependencyAwareValidator.cs
using Microsoft.Extensions.Logging;
using Ruleflow.NET.Engine.Validation.Core.Context;
using Ruleflow.NET.Engine.Validation.Core.Results;
using Ruleflow.NET.Engine.Validation.Core.Validators.Execution;
using Ruleflow.NET.Engine.Validation.Enums;
using Ruleflow.NET.Engine.Validation.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Ruleflow.NET.Engine.Validation.Core.Validators
{
    /// <summary>
    /// Validátor podporující závislosti mezi pravidly a kontextovou validaci.
    /// </summary>
    public class DependencyAwareValidator<T> : IValidator<T>, IContextAwareValidator<T>
    {
        private readonly IEnumerable<IValidationRule<T>> _rules;
        private readonly ILogger<DependencyAwareValidator<T>>? _logger;
        private readonly RuleExecutor<T> _ruleExecutor;

        /// <summary>
        /// Inicializuje novou instanci validátoru s podporou závislostí.
        /// </summary>
        /// <param name="rules">Kolekce pravidel pro validaci</param>
        /// <param name="logger">Logger pro zaznamenávání průběhu validace</param>
        /// <exception cref="ArgumentNullException">Vyhozeno, pokud je parametr rules null</exception>
        /// <exception cref="InvalidOperationException">Vyhozeno, pokud je detekována cyklická závislost mezi pravidly</exception>
        public DependencyAwareValidator(IEnumerable<IValidationRule<T>> rules, ILogger<DependencyAwareValidator<T>>? logger = null)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            _logger = logger;
            _ruleExecutor = new RuleExecutor<T>(logger);

            // Validujeme graf závislostí, pokud existují závislá pravidla
            var dependentRules = rules.OfType<IDependentValidationRule<T>>().ToList();
            if (dependentRules.Any())
            {
                // Použijeme plánování pouze pro validaci grafu závislostí
                var planner = new RuleExecutionPlanner<T>(rules);
            }
        }

        /// <summary>
        /// Validuje vstupní data a vrátí objekt s výsledky validace.
        /// </summary>
        /// <param name="input">Vstupní data k validaci</param>
        /// <returns>Validační výsledek obsahující všechny nalezené chyby</returns>
        public IValidationResult CollectValidationResults(T input)
        {
            var context = new ValidationContext();
            return ValidateWithContext(input, context);
        }

        /// <summary>
        /// Validuje vstupní data s použitím daného kontextu.
        /// </summary>
        /// <param name="input">Vstupní data k validaci</param>
        /// <param name="context">Validační kontext pro sdílení stavu mezi pravidly</param>
        /// <returns>Validační výsledek obsahující všechny nalezené chyby</returns>
        public IValidationResult ValidateWithContext(T input, ValidationContext context)
        {
            var result = new ValidationResult();

            try
            {
                // Vytvoření plánu vykonávání pravidel
                var planner = new RuleExecutionPlanner<T>(_rules);

                // Nejprve spustíme nezávislá pravidla
                var independentRulesPlan = planner.CreateIndependentRulesPlan();
                _logger?.LogDebug("Spouštění {Count} nezávislých pravidel", independentRulesPlan.Count);
                _ruleExecutor.ExecuteRules(independentRulesPlan, input, result, context);

                // Poté spustíme závislá pravidla v topologickém pořadí
                var dependentRulesPlan = planner.CreateDependentRulesPlan();
                _logger?.LogDebug("Spouštění {Count} závislých pravidel", dependentRulesPlan.Count);
                _ruleExecutor.ExecuteDependentRules(dependentRulesPlan, input, result, context);
            }
            catch (OperationCanceledException)
            {
                _logger?.LogInformation("Validace byla zrušena");
                result.AddError("Validace byla zrušena uživatelem", ValidationSeverity.Information);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("cirkulární"))
            {
                _logger?.LogError("Chyba při topologickém řazení: {Message}", ex.Message);
                result.AddError(ex.Message, ValidationSeverity.Error, "CIRCULAR_DEPENDENCY");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Neočekávaná chyba během validace: {Message}", ex.Message);
                result.AddError($"Neočekávaná chyba během validace: {ex.Message}", ValidationSeverity.Critical, "UNEXPECTED_ERROR", ex);
            }

            return result;
        }

        /// <summary>
        /// Validuje vstupní data a vyhodí výjimku při selhání validace.
        /// </summary>
        /// <param name="input">Vstupní data k validaci</param>
        /// <exception cref="AggregateException">Vyhozeno při selhání validace</exception>
        public void ValidateOrThrow(T input)
        {
            var result = CollectValidationResults(input);
            result.ThrowIfInvalid();
        }

        /// <summary>
        /// Validuje vstup podle zadaného režimu.
        /// </summary>
        /// <param name="input">Vstupní data k validaci</param>
        /// <param name="mode">Režim validace určující chování při selhání</param>
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

        /// <summary>
        /// Validuje vstup a vrátí výsledek validace - pro zpětnou kompatibilitu.
        /// </summary>
        /// <param name="input">Vstupní data k validaci</param>
        /// <returns>Validační výsledek</returns>
        public IValidationResult ValidateWithResult(T input)
        {
            return CollectValidationResults(input);
        }
    }
}
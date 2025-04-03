using Microsoft.Extensions.Logging;
using Ruleflow.NET.Engine.Validation.Enums;
using Ruleflow.NET.Engine.Validation.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ruleflow.NET.Engine.Validation
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

            // Validujeme graf závislostí, pokud existují závislá pravidla
            var dependentRules = rules.OfType<IDependentValidationRule<T>>().ToList();
            if (dependentRules.Any())
            {
                ValidateDependencyGraph(dependentRules);
            }
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

            // Seřazení pravidel: nejprve podle priority
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

            // Pro závislá pravidla použijeme topologické řazení
            if (dependentRules.Any())
            {
                try
                {
                    ProcessDependentRulesWithTopologicalSort(dependentRules, input, result, context);
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("cirkulární"))
                {
                    _logger?.LogError("Chyba při topologickém řazení: {Message}", ex.Message);
                    result.AddError(ex.Message, ValidationSeverity.Error, "CIRCULAR_DEPENDENCY");

                    // Fallback na původní metodu, která se pokusí zpracovat co nejvíce pravidel
                    ProcessDependentRulesIteratively(dependentRules, input, result, context);
                }
            }

            return result;
        }

        private void ProcessDependentRulesWithTopologicalSort(
            IEnumerable<IDependentValidationRule<T>> dependentRules,
            T input,
            ValidationResult result,
            ValidationContext context)
        {
            // Vytvoříme graf pro topologické řazení
            var graph = CreateDependencyGraph(dependentRules);

            // Vytvoříme mapování ID pravidel na samotná pravidla
            var ruleMap = dependentRules.ToDictionary(
                rule => GetRuleId(rule),
                rule => rule);

            // Provedeme topologické řazení
            var sortedRules = TopologicalSort(graph);

            // Zpracujeme pravidla v topologickém pořadí
            foreach (var ruleId in sortedRules)
            {
                if (ruleMap.TryGetValue(ruleId, out var rule))
                {
                    // Podmíněné vyhodnocení závislostí
                    if (ShouldProcessRule(rule, context))
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
                    else
                    {
                        _logger?.LogDebug(
                            "Pravidlo {RuleName} přeskočeno, závislosti nebyly splněny",
                            rule.GetType().Name);
                    }
                }
            }
        }

        private Dictionary<string, List<string>> CreateDependencyGraph(IEnumerable<IDependentValidationRule<T>> rules)
        {
            var graph = new Dictionary<string, List<string>>();

            // Nejprve přidáme všechny uzly (i ty bez závislostí)
            foreach (var rule in rules)
            {
                var ruleId = GetRuleId(rule);
                if (!graph.ContainsKey(ruleId))
                {
                    graph[ruleId] = new List<string>();
                }
            }

            // Poté přidáme závislosti (převrácené pro účely topologického řazení)
            // Pro topologické řazení A->B znamená, že B závisí na A (B má na A hranu)
            foreach (var rule in rules)
            {
                var ruleId = GetRuleId(rule);

                foreach (var dependencyId in rule.DependsOn)
                {
                    // Pokud závislost neexistuje v grafu, přidáme ji
                    if (!graph.ContainsKey(dependencyId))
                    {
                        graph[dependencyId] = new List<string>();
                    }

                    // Přidáme hranu od závislosti k pravidlu (obrácený směr)
                    graph[dependencyId].Add(ruleId);
                }
            }

            return graph;
        }

        private List<string> TopologicalSort(Dictionary<string, List<string>> graph)
        {
            var result = new List<string>();
            var visited = new HashSet<string>();
            var temporaryMarks = new HashSet<string>();

            foreach (var node in graph.Keys)
            {
                if (!visited.Contains(node))
                {
                    TopologicalSortVisit(node, graph, temporaryMarks, visited, result);
                }
            }

            return result;
        }

        private void TopologicalSortVisit(
            string node,
            Dictionary<string, List<string>> graph,
            HashSet<string> temporaryMarks,
            HashSet<string> visited,
            List<string> result)
        {
            // Detekce cyklu
            if (temporaryMarks.Contains(node))
            {
                throw new InvalidOperationException($"Detekována cirkulární závislost v pravidle {node}");
            }

            if (!visited.Contains(node))
            {
                temporaryMarks.Add(node);

                // Projdeme všechny sousedy v grafu
                if (graph.TryGetValue(node, out var edges))
                {
                    foreach (var neighbor in edges)
                    {
                        TopologicalSortVisit(neighbor, graph, temporaryMarks, visited, result);
                    }
                }

                temporaryMarks.Remove(node);
                visited.Add(node);
                result.Add(node);
            }
        }

        private void ProcessDependentRulesIteratively(
            IEnumerable<IDependentValidationRule<T>> dependentRules,
            T input,
            ValidationResult result,
            ValidationContext context)
        {
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
                    // Přeskočit podmíněná pravidla, která nesplňují podmínku
                    if (rule is IConditionalValidationRule<T> conditionalRule &&
                        !conditionalRule.ShouldValidate(input, context))
                    {
                        _logger?.LogDebug("Pravidlo {RuleName} přeskočeno, podmínka nebyla splněna",
                            rule.GetType().Name);

                        // Odstraníme pravidlo ze seznamu zbývajících, i když jsme ho nezpracovali
                        remainingRules.Remove(rule);
                        continue;
                    }

                    ProcessRule(rule, input, result, context);
                    remainingRules.Remove(rule);
                }

            } while (remainingRules.Count > 0 && remainingRules.Count < previousCount);

            // Přidání informace o neprocesovaných pravidlech
            foreach (var rule in remainingRules)
            {
                _logger?.LogWarning(
                    "Pravidlo {RuleId} nebylo vyhodnoceno, protože jeho závislosti nebyly splněny",
                    GetRuleId(rule));
            }
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

        private void ValidateDependencyGraph(IEnumerable<IDependentValidationRule<T>> rules)
        {
            // Vytvoření grafu závislostí pomocí Dictionary
            var dependencyGraph = rules.ToDictionary(
                rule => GetRuleId(rule),
                rule => rule.DependsOn.ToList());

            // Pro každé pravidlo spustíme detekci cyklů
            foreach (var rule in rules)
            {
                var visited = new HashSet<string>();
                var inProcess = new HashSet<string>();

                if (ContainsCycle(GetRuleId(rule), dependencyGraph, visited, inProcess, out var cycle))
                {
                    // Sestavíme informativní zprávu o cyklu
                    var cycleDescription = string.Join(" -> ", cycle) + " -> " + cycle[0];
                    throw new InvalidOperationException(
                        $"Detekována cirkulární závislost mezi pravidly: {cycleDescription}");
                }
            }
        }

        private bool ContainsCycle(
            string currentRule,
            Dictionary<string, List<string>> graph,
            HashSet<string> visited,
            HashSet<string> inProcess,
            out List<string> cycle)
        {
            cycle = new List<string>();

            // Pokud jsme již pravidlo zpracovali a není v cyklu, můžeme přeskočit
            if (visited.Contains(currentRule))
                return false;

            // Pokud je pravidlo aktuálně zpracováváno, nalezli jsme cyklus
            if (inProcess.Contains(currentRule))
            {
                cycle.Add(currentRule);
                return true;
            }

            // Označíme pravidlo jako zpracovávané
            inProcess.Add(currentRule);

            // Rekurzivně kontrolujeme všechny závislosti
            if (graph.TryGetValue(currentRule, out var dependencies))
            {
                foreach (var dependency in dependencies)
                {
                    if (ContainsCycle(dependency, graph, visited, inProcess, out var subCycle))
                    {
                        // Přidáme aktuální pravidlo do cyklu, pokud jsme našli cyklus v podstromu
                        cycle.Add(currentRule);
                        cycle.AddRange(subCycle);
                        return true;
                    }
                }
            }

            // Pravidlo je zpracováno a není součástí cyklu
            inProcess.Remove(currentRule);
            visited.Add(currentRule);

            return false;
        }

        private string GetRuleId(IValidationRule<T> rule)
        {
            return rule is IIdentifiableValidationRule<T> identifiable
                ? identifiable.RuleId
                : rule.GetType().FullName ?? rule.GetType().Name;
        }
    }
}
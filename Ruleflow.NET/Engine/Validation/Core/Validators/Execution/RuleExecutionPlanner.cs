// Engine/Validation/Core/Validators/Execution/RuleExecutionPlanner.cs
using Ruleflow.NET.Engine.Validation.Core.Validators.Dependency;
using Ruleflow.NET.Engine.Validation.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ruleflow.NET.Engine.Validation.Core.Validators.Execution
{
    /// <summary>
    /// Plánuje pořadí vykonávání validačních pravidel s ohledem na jejich závislosti a priority.
    /// </summary>
    /// <typeparam name="T">Typ validovaných dat</typeparam>
    internal class RuleExecutionPlanner<T>
    {
        private readonly DependencyGraph<T> _dependencyGraph;
        private readonly List<IValidationRule<T>> _independentRules = new();
        private readonly List<IDependentValidationRule<T>> _dependentRules = new();

        /// <summary>
        /// Inicializuje novou instanci plánovače vykonávání pravidel.
        /// </summary>
        /// <param name="rules">Kolekce pravidel</param>
        public RuleExecutionPlanner(IEnumerable<IValidationRule<T>> rules)
        {
            // Rozdělení pravidel na nezávislá a závislá
            foreach (var rule in rules)
            {
                if (rule is IDependentValidationRule<T> dependentRule)
                {
                    _dependentRules.Add(dependentRule);
                }
                else
                {
                    _independentRules.Add(rule);
                }
            }

            // Vytvoření grafu závislostí pro závislá pravidla
            _dependencyGraph = DependencyGraph<T>.BuildFrom(_dependentRules);
            _dependencyGraph.ValidateNoCycles(); // Kontrola cyklů
        }

        /// <summary>
        /// Vytvoří plán vykonávání pro nezávislá pravidla.
        /// </summary>
        /// <returns>Seznam nezávislých pravidel seřazených podle priority</returns>
        public List<IValidationRule<T>> CreateIndependentRulesPlan()
        {
            // Seřazení pravidel podle priority (vyšší priorita = dřívější vyhodnocení)
            return _independentRules
                .OrderByDescending(GetRulePriority)
                .ToList();
        }

        /// <summary>
        /// Vytvoří plán vykonávání pro závislá pravidla.
        /// </summary>
        /// <returns>Seznam závislých pravidel v topologickém pořadí a zohledňující prioritu</returns>
        public List<IDependentValidationRule<T>> CreateDependentRulesPlan()
        {
            // Topologické řazení závislých pravidel
            var sortedRuleIds = _dependencyGraph.TopologicalSort();

            // Získání pravidel podle ID a jejich seřazení podle priority
            var topologicallySortedRules = sortedRuleIds
                .Select(id => _dependencyGraph.GetRule(id))
                .OfType<IDependentValidationRule<T>>()
                .ToList();

            // Pokud mají pravidla stejné pořadí v topologickém řazení (např. nezávislá na sobě),
            // seřadíme je ještě podle priority
            var result = new List<IDependentValidationRule<T>>();

            // Přidáme pravidla ve správném pořadí, respektujícím jak topologické řazení,
            // tak prioritu pravidel se stejnou topologickou úrovní
            foreach (var rule in topologicallySortedRules)
            {
                result.Add(rule);
            }

            return result;
        }

        /// <summary>
        /// Získá prioritu pravidla.
        /// </summary>
        /// <param name="rule">Pravidlo</param>
        /// <returns>Priorita pravidla, nebo 0, pokud pravidlo nepodporuje prioritu</returns>
        private static int GetRulePriority(IValidationRule<T> rule)
        {
            return rule is IPrioritizedValidationRule<T> prioritized
                ? prioritized.Priority
                : 0;
        }
    }
}
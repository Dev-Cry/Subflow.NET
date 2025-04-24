// Engine/Validation/Core/Validators/Dependency/DependencyGraph.cs
using Ruleflow.NET.Engine.Validation.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ruleflow.NET.Engine.Validation.Core.Validators.Dependency
{
    /// <summary>
    /// Reprezentuje graf závislostí mezi validačními pravidly a poskytuje metody pro jeho analýzu.
    /// </summary>
    /// <typeparam name="T">Typ validovaných dat</typeparam>
    internal class DependencyGraph<T>
    {
        private readonly Dictionary<string, List<string>> _graph = new();
        private readonly Dictionary<string, IValidationRule<T>> _ruleMap = new();

        /// <summary>
        /// Vytvoří graf závislostí z kolekce pravidel.
        /// </summary>
        /// <param name="rules">Kolekce pravidel</param>
        /// <returns>Vytvořený graf závislostí</returns>
        public static DependencyGraph<T> BuildFrom(IEnumerable<IValidationRule<T>> rules)
        {
            var graph = new DependencyGraph<T>();
            graph.BuildGraph(rules);
            return graph;
        }

        /// <summary>
        /// Přidá pravidlo do grafu.
        /// </summary>
        /// <param name="rule">Pravidlo k přidání</param>
        public void AddRule(IValidationRule<T> rule)
        {
            var ruleId = GetRuleId(rule);

            if (!_graph.ContainsKey(ruleId))
            {
                _graph[ruleId] = new List<string>();
            }

            _ruleMap[ruleId] = rule;

            // Pokud je to závislé pravidlo, přidáme hrany
            if (rule is IDependentValidationRule<T> dependentRule)
            {
                foreach (var dependencyId in dependentRule.DependsOn)
                {
                    if (!_graph.ContainsKey(dependencyId))
                    {
                        _graph[dependencyId] = new List<string>();
                    }

                    // Přidáme hranu od závislosti k pravidlu (pro topologické řazení)
                    _graph[dependencyId].Add(ruleId);
                }
            }
        }

        /// <summary>
        /// Vytvoří graf z kolekce pravidel.
        /// </summary>
        /// <param name="rules">Kolekce pravidel</param>
        public void BuildGraph(IEnumerable<IValidationRule<T>> rules)
        {
            // Nejprve přidáme všechna pravidla do grafu
            foreach (var rule in rules)
            {
                AddRule(rule);
            }
        }

        /// <summary>
        /// Zkontroluje, zda graf obsahuje cykly.
        /// </summary>
        /// <returns>True, pokud graf obsahuje cyklus; jinak false</returns>
        /// <exception cref="InvalidOperationException">Vyhozeno, pokud je detekován cyklus</exception>
        public bool ValidateNoCycles()
        {
            var visited = new HashSet<string>();
            var inProcess = new HashSet<string>();

            foreach (var node in _graph.Keys)
            {
                if (!visited.Contains(node) && ContainsCycle(node, visited, inProcess, out var cycle))
                {
                    var cycleDescription = string.Join(" -> ", cycle) + " -> " + cycle[0];
                    throw new InvalidOperationException(
                        $"Detekována cirkulární závislost mezi pravidly: {cycleDescription}");
                }
            }

            return true;
        }

        /// <summary>
        /// Provede topologické řazení grafu.
        /// </summary>
        /// <returns>Seznam ID pravidel v topologickém pořadí</returns>
        /// <exception cref="InvalidOperationException">Vyhozeno, pokud je detekován cyklus</exception>
        public List<string> TopologicalSort()
        {
            var result = new List<string>();
            var visited = new HashSet<string>();
            var temporaryMarks = new HashSet<string>();

            foreach (var node in _graph.Keys)
            {
                if (!visited.Contains(node))
                {
                    TopologicalSortVisit(node, temporaryMarks, visited, result);
                }
            }

            return result;
        }

        /// <summary>
        /// Získá pravidlo podle jeho ID.
        /// </summary>
        /// <param name="ruleId">ID pravidla</param>
        /// <returns>Pravidlo s daným ID, nebo null, pokud pravidlo neexistuje</returns>
        public IValidationRule<T> GetRule(string ruleId)
        {
            return _ruleMap.TryGetValue(ruleId, out var rule) ? rule : null;
        }

        /// <summary>
        /// Získá všechna pravidla ze seznamu ID.
        /// </summary>
        /// <param name="ruleIds">Seznam ID pravidel</param>
        /// <returns>Seznam pravidel</returns>
        public List<IValidationRule<T>> GetRules(IEnumerable<string> ruleIds)
        {
            return ruleIds
                .Where(id => _ruleMap.ContainsKey(id))
                .Select(id => _ruleMap[id])
                .ToList();
        }

        /// <summary>
        /// Rekurzivně zkontroluje, zda daný uzel grafu je součástí cyklu.
        /// </summary>
        private bool ContainsCycle(
            string currentNode,
            HashSet<string> visited,
            HashSet<string> inProcess,
            out List<string> cycle)
        {
            cycle = new List<string>();

            // Pokud jsme již uzel zpracovali a není v cyklu, můžeme přeskočit
            if (visited.Contains(currentNode))
                return false;

            // Pokud je uzel aktuálně zpracováván, nalezli jsme cyklus
            if (inProcess.Contains(currentNode))
            {
                cycle.Add(currentNode);
                return true;
            }

            // Označíme uzel jako zpracovávaný
            inProcess.Add(currentNode);

            // Rekurzivně kontrolujeme všechny závislosti
            if (_graph.TryGetValue(currentNode, out var edges))
            {
                foreach (var dependency in edges)
                {
                    if (ContainsCycle(dependency, visited, inProcess, out var subCycle))
                    {
                        // Přidáme aktuální uzel do cyklu, pokud jsme našli cyklus v podstromu
                        cycle.Add(currentNode);
                        cycle.AddRange(subCycle);
                        return true;
                    }
                }
            }

            // Uzel je zpracován a není součástí cyklu
            inProcess.Remove(currentNode);
            visited.Add(currentNode);

            return false;
        }

        /// <summary>
        /// Pomocná metoda pro topologické řazení grafu.
        /// </summary>
        private void TopologicalSortVisit(
            string node,
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
                if (_graph.TryGetValue(node, out var edges))
                {
                    foreach (var neighbor in edges)
                    {
                        TopologicalSortVisit(neighbor, temporaryMarks, visited, result);
                    }
                }

                temporaryMarks.Remove(node);
                visited.Add(node);
                result.Add(node);
            }
        }

        /// <summary>
        /// Pomocná metoda pro získání ID pravidla.
        /// </summary>
        private static string GetRuleId(IValidationRule<T> rule)
        {
            return rule is IIdentifiableValidationRule<T> identifiable
                ? identifiable.RuleId
                : rule.GetType().FullName ?? rule.GetType().Name;
        }
    }
}
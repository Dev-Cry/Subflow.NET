using System;
using System.Collections.Generic;
using System.Linq;
using Ruleflow.NET.Engine.Models.Rule.Interface;
using Ruleflow.NET.Engine.Models.Rule.Type.Interface;
using Ruleflow.NET.Engine.Registry.Interface;

namespace Ruleflow.NET.Engine.Registry
{
    /// <summary>
    /// Registr pravidel, který uchovává pravidla v paměti a poskytuje rychlý přístup k nim.
    /// </summary>
    /// <typeparam name="TInput">Typ validovaných dat.</typeparam>
    public class RuleRegistry<TInput> : IRuleRegistry<TInput>
    {
        // Základní úložiště pro všechna pravidla
        private readonly Dictionary<string, IRule<TInput>> _rulesById = new Dictionary<string, IRule<TInput>>();

        // Indexy pro rychlý přístup
        private readonly Dictionary<int, IRule<TInput>> _rulesByInternalId = new Dictionary<int, IRule<TInput>>();
        private readonly Dictionary<string, List<IRule<TInput>>> _rulesByName = new Dictionary<string, List<IRule<TInput>>>();
        private readonly Dictionary<int, List<IRule<TInput>>> _rulesByType = new Dictionary<int, List<IRule<TInput>>>();
        private readonly SortedDictionary<int, List<IRule<TInput>>> _rulesByPriority = new SortedDictionary<int, List<IRule<TInput>>>(Comparer<int>.Create((a, b) => b.CompareTo(a))); // Sestupně podle priority
        private readonly Dictionary<bool, List<IRule<TInput>>> _rulesByActiveStatus = new Dictionary<bool, List<IRule<TInput>>>
        {
            { true, new List<IRule<TInput>>() },
            { false, new List<IRule<TInput>>() }
        };

        /// <summary>
        /// Vrátí počet registrovaných pravidel.
        /// </summary>
        public int Count => _rulesById.Count;

        /// <summary>
        /// Získá všechna registrovaná pravidla.
        /// </summary>
        public IReadOnlyList<IRule<TInput>> AllRules => _rulesById.Values.ToList();

        /// <summary>
        /// Vytvoří novou instanci registru pravidel.
        /// </summary>
        public RuleRegistry()
        {
        }

        /// <summary>
        /// Vytvoří novou instanci registru pravidel s počátečními pravidly.
        /// </summary>
        /// <param name="initialRules">Počáteční pravidla, která budou registrována.</param>
        public RuleRegistry(IEnumerable<IRule<TInput>> initialRules)
        {
            if (initialRules != null)
            {
                foreach (var rule in initialRules)
                {
                    RegisterRule(rule);
                }
            }
        }

        /// <summary>
        /// Registruje pravidlo v registru.
        /// </summary>
        /// <param name="rule">Pravidlo, které má být registrováno.</param>
        /// <returns>True pokud bylo pravidlo úspěšně registrováno, jinak false (např. pokud již existuje pravidlo se stejným ID).</returns>
        public bool RegisterRule(IRule<TInput> rule)
        {
            if (rule == null)
                throw new ArgumentNullException(nameof(rule));

            if (_rulesById.ContainsKey(rule.RuleId))
                return false;

            // Přidání do základního úložiště
            _rulesById[rule.RuleId] = rule;
            _rulesByInternalId[rule.Id] = rule;

            // Indexace podle názvu
            if (!string.IsNullOrEmpty(rule.Name))
            {
                if (!_rulesByName.TryGetValue(rule.Name, out var nameList))
                {
                    nameList = new List<IRule<TInput>>();
                    _rulesByName[rule.Name] = nameList;
                }
                nameList.Add(rule);
            }

            // Indexace podle typu
            if (!_rulesByType.TryGetValue(rule.RuleTypeId, out var typeList))
            {
                typeList = new List<IRule<TInput>>();
                _rulesByType[rule.RuleTypeId] = typeList;
            }
            typeList.Add(rule);

            // Indexace podle priority
            if (!_rulesByPriority.TryGetValue(rule.Priority, out var priorityList))
            {
                priorityList = new List<IRule<TInput>>();
                _rulesByPriority[rule.Priority] = priorityList;
            }
            priorityList.Add(rule);

            // Indexace podle aktivního stavu
            _rulesByActiveStatus[rule.IsActive].Add(rule);

            return true;
        }

        /// <summary>
        /// Odregistruje pravidlo z registru.
        /// </summary>
        /// <param name="ruleId">ID pravidla, které má být odregistrováno.</param>
        /// <returns>True pokud bylo pravidlo úspěšně odregistrováno, jinak false (např. pokud pravidlo neexistuje).</returns>
        public bool UnregisterRule(string ruleId)
        {
            if (string.IsNullOrEmpty(ruleId))
                throw new ArgumentException("ID pravidla nemůže být prázdné.", nameof(ruleId));

            if (!_rulesById.TryGetValue(ruleId, out var rule))
                return false;

            // Odstranění ze základního úložiště
            _rulesById.Remove(ruleId);
            _rulesByInternalId.Remove(rule.Id);

            // Odstranění z indexu podle názvu
            if (!string.IsNullOrEmpty(rule.Name) && _rulesByName.TryGetValue(rule.Name, out var nameList))
            {
                nameList.Remove(rule);
                if (nameList.Count == 0)
                    _rulesByName.Remove(rule.Name);
            }

            // Odstranění z indexu podle typu
            if (_rulesByType.TryGetValue(rule.RuleTypeId, out var typeList))
            {
                typeList.Remove(rule);
                if (typeList.Count == 0)
                    _rulesByType.Remove(rule.RuleTypeId);
            }

            // Odstranění z indexu podle priority
            if (_rulesByPriority.TryGetValue(rule.Priority, out var priorityList))
            {
                priorityList.Remove(rule);
                if (priorityList.Count == 0)
                    _rulesByPriority.Remove(rule.Priority);
            }

            // Odstranění z indexu podle aktivního stavu
            _rulesByActiveStatus[rule.IsActive].Remove(rule);

            return true;
        }

        /// <summary>
        /// Aktualizuje pravidlo v registru.
        /// </summary>
        /// <param name="rule">Aktualizované pravidlo.</param>
        /// <returns>True pokud bylo pravidlo úspěšně aktualizováno, jinak false (např. pokud pravidlo neexistuje).</returns>
        public bool UpdateRule(IRule<TInput> rule)
        {
            if (rule == null)
                throw new ArgumentNullException(nameof(rule));

            // Pro jednoduchost nejprve odregistrujeme a poté znovu zaregistrujeme
            if (UnregisterRule(rule.RuleId))
            {
                return RegisterRule(rule);
            }

            return false;
        }

        /// <summary>
        /// Získá pravidlo podle jeho ID.
        /// </summary>
        /// <param name="ruleId">ID pravidla.</param>
        /// <returns>Pravidlo s daným ID nebo null, pokud pravidlo neexistuje.</returns>
        public IRule<TInput>? GetRuleById(string ruleId)
        {
            if (string.IsNullOrEmpty(ruleId))
                throw new ArgumentException("ID pravidla nemůže být prázdné.", nameof(ruleId));

            _rulesById.TryGetValue(ruleId, out var rule);
            return rule;
        }

        /// <summary>
        /// Získá pravidlo podle jeho interního ID.
        /// </summary>
        /// <param name="internalId">Interní ID pravidla.</param>
        /// <returns>Pravidlo s daným interním ID nebo null, pokud pravidlo neexistuje.</returns>
        public IRule<TInput>? GetRuleByInternalId(int internalId)
        {
            _rulesByInternalId.TryGetValue(internalId, out var rule);
            return rule;
        }

        /// <summary>
        /// Získá pravidla podle jejich názvu.
        /// </summary>
        /// <param name="name">Název pravidla.</param>
        /// <returns>Seznam pravidel s daným názvem nebo prázdný seznam, pokud žádná pravidla neexistují.</returns>
        public IReadOnlyList<IRule<TInput>> GetRulesByName(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Název pravidla nemůže být prázdný.", nameof(name));

            if (_rulesByName.TryGetValue(name, out var rules))
                return rules;

            return Array.Empty<IRule<TInput>>();
        }

        /// <summary>
        /// Získá pravidla podle jejich typu.
        /// </summary>
        /// <param name="ruleTypeId">ID typu pravidla.</param>
        /// <returns>Seznam pravidel daného typu nebo prázdný seznam, pokud žádná pravidla neexistují.</returns>
        public IReadOnlyList<IRule<TInput>> GetRulesByType(int ruleTypeId)
        {
            if (_rulesByType.TryGetValue(ruleTypeId, out var rules))
                return rules;

            return Array.Empty<IRule<TInput>>();
        }

        /// <summary>
        /// Získá pravidla podle jejich typu.
        /// </summary>
        /// <param name="ruleType">Typ pravidla.</param>
        /// <returns>Seznam pravidel daného typu nebo prázdný seznam, pokud žádná pravidla neexistují.</returns>
        public IReadOnlyList<IRule<TInput>> GetRulesByType(IRuleType ruleType)
        {
            if (ruleType == null)
                throw new ArgumentNullException(nameof(ruleType));

            return GetRulesByType(ruleType.Id);
        }

        /// <summary>
        /// Získá pravidla podle priority.
        /// </summary>
        /// <param name="priority">Priorita pravidel.</param>
        /// <returns>Seznam pravidel s danou prioritou nebo prázdný seznam, pokud žádná pravidla neexistují.</returns>
        public IReadOnlyList<IRule<TInput>> GetRulesByPriority(int priority)
        {
            if (_rulesByPriority.TryGetValue(priority, out var rules))
                return rules;

            return Array.Empty<IRule<TInput>>();
        }

        /// <summary>
        /// Získá pravidla seřazená podle priority (sestupně).
        /// </summary>
        /// <returns>Seznam pravidel seřazených podle priority.</returns>
        public IReadOnlyList<IRule<TInput>> GetRulesByPriorityOrder()
        {
            var result = new List<IRule<TInput>>();

            foreach (var priorityGroup in _rulesByPriority)
            {
                result.AddRange(priorityGroup.Value);
            }

            return result;
        }

        /// <summary>
        /// Získá pravidla podle jejich aktivního stavu.
        /// </summary>
        /// <param name="isActive">Aktivní stav pravidel.</param>
        /// <returns>Seznam pravidel s daným aktivním stavem.</returns>
        public IReadOnlyList<IRule<TInput>> GetRulesByActiveStatus(bool isActive)
        {
            return _rulesByActiveStatus[isActive];
        }

        /// <summary>
        /// Získá aktivní pravidla.
        /// </summary>
        /// <returns>Seznam aktivních pravidel.</returns>
        public IReadOnlyList<IRule<TInput>> GetActiveRules()
        {
            return _rulesByActiveStatus[true];
        }

        /// <summary>
        /// Získá aktivní pravidla seřazená podle priority (sestupně).
        /// </summary>
        /// <returns>Seznam aktivních pravidel seřazených podle priority.</returns>
        public IReadOnlyList<IRule<TInput>> GetActiveRulesByPriorityOrder()
        {
            var result = new List<IRule<TInput>>();

            foreach (var priorityGroup in _rulesByPriority)
            {
                foreach (var rule in priorityGroup.Value)
                {
                    if (rule.IsActive)
                        result.Add(rule);
                }
            }

            return result;
        }

        /// <summary>
        /// Získá pravidla na základě vlastního predikátu.
        /// </summary>
        /// <param name="predicate">Predikát pro filtrování pravidel.</param>
        /// <returns>Seznam pravidel splňujících daný predikát.</returns>
        public IReadOnlyList<IRule<TInput>> GetRulesByPredicate(Func<IRule<TInput>, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return _rulesById.Values.Where(predicate).ToList();
        }

        /// <summary>
        /// Vyčistí registr pravidel.
        /// </summary>
        public void Clear()
        {
            _rulesById.Clear();
            _rulesByInternalId.Clear();
            _rulesByName.Clear();
            _rulesByType.Clear();
            _rulesByPriority.Clear();
            _rulesByActiveStatus[true].Clear();
            _rulesByActiveStatus[false].Clear();
        }
    }
}
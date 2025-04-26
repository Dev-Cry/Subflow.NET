using System;
using System.Collections.Generic;
using System.Linq;
using Ruleflow.NET.Engine.Models.Rule.Interface;
using Ruleflow.NET.Engine.Models.Rule.Type;
using Ruleflow.NET.Engine.Models.Rule.Type.Interface;
using Ruleflow.NET.Engine.Models.Rule.Group;
using Ruleflow.NET.Engine.Models.Rule.Group.Interface;
using Ruleflow.NET.Engine.Registry.Interface;
using Ruleflow.NET.Engine.Models.Rule;

namespace Ruleflow.NET.Engine.Registry
{
    /// <summary>
    /// Rozšířený registr pravidel, který uchovává pravidla, jejich skupiny a typy v paměti a poskytuje rychlý přístup k nim.
    /// </summary>
    /// <typeparam name="TInput">Typ validovaných dat.</typeparam>
    public class RuleRegistry<TInput> : IRuleRegistry<TInput>
    {
        #region Základní úložiště pravidel

        // Základní úložiště pro všechna pravidla
        private readonly Dictionary<string, IRule<TInput>> _rulesById = new Dictionary<string, IRule<TInput>>();

        // Indexy pro rychlý přístup k pravidlům
        private readonly Dictionary<int, IRule<TInput>> _rulesByInternalId = new Dictionary<int, IRule<TInput>>();
        private readonly Dictionary<string, List<IRule<TInput>>> _rulesByName = new Dictionary<string, List<IRule<TInput>>>();
        private readonly Dictionary<int, List<IRule<TInput>>> _rulesByType = new Dictionary<int, List<IRule<TInput>>>();
        private readonly SortedDictionary<int, List<IRule<TInput>>> _rulesByPriority = new SortedDictionary<int, List<IRule<TInput>>>(Comparer<int>.Create((a, b) => b.CompareTo(a))); // Sestupně podle priority
        private readonly Dictionary<bool, List<IRule<TInput>>> _rulesByActiveStatus = new Dictionary<bool, List<IRule<TInput>>>
        {
            { true, new List<IRule<TInput>>() },
            { false, new List<IRule<TInput>>() }
        };

        #endregion

        #region Úložiště pro typy pravidel

        // Úložiště pro typy pravidel
        private readonly Dictionary<int, IRuleType<TInput>> _ruleTypesById = new Dictionary<int, IRuleType<TInput>>();
        private readonly Dictionary<string, IRuleType<TInput>> _ruleTypesByCode = new Dictionary<string, IRuleType<TInput>>();
        private readonly Dictionary<bool, List<IRuleType<TInput>>> _ruleTypesByStatus = new Dictionary<bool, List<IRuleType<TInput>>>
        {
            { true, new List<IRuleType<TInput>>() },
            { false, new List<IRuleType<TInput>>() }
        };

        #endregion

        #region Úložiště pro skupiny pravidel

        // Úložiště pro skupiny pravidel
        private readonly Dictionary<string, IRuleGroup<TInput>> _ruleGroupsById = new Dictionary<string, IRuleGroup<TInput>>();
        private readonly Dictionary<int, IRuleGroup<TInput>> _ruleGroupsByInternalId = new Dictionary<int, IRuleGroup<TInput>>();
        private readonly Dictionary<string, List<IRuleGroup<TInput>>> _ruleGroupsByName = new Dictionary<string, List<IRuleGroup<TInput>>>();
        private readonly Dictionary<int, List<IRuleGroup<TInput>>> _ruleGroupsByType = new Dictionary<int, List<IRuleGroup<TInput>>>();
        private readonly Dictionary<bool, List<IRuleGroup<TInput>>> _ruleGroupsByActiveStatus = new Dictionary<bool, List<IRuleGroup<TInput>>>
        {
            { true, new List<IRuleGroup<TInput>>() },
            { false, new List<IRuleGroup<TInput>>() }
        };

        #endregion

        #region Úložiště pro typy skupin pravidel

        // Úložiště pro typy skupin pravidel
        private readonly Dictionary<int, IRuleGroupType<TInput>> _ruleGroupTypesById = new Dictionary<int, IRuleGroupType<TInput>>();
        private readonly Dictionary<string, IRuleGroupType<TInput>> _ruleGroupTypesByCode = new Dictionary<string, IRuleGroupType<TInput>>();
        private readonly Dictionary<bool, List<IRuleGroupType<TInput>>> _ruleGroupTypesByStatus = new Dictionary<bool, List<IRuleGroupType<TInput>>>
        {
            { true, new List<IRuleGroupType<TInput>>() },
            { false, new List<IRuleGroupType<TInput>>() }
        };

        #endregion

        #region Vlastnosti registru

        /// <summary>
        /// Vrátí počet registrovaných pravidel.
        /// </summary>
        public int Count => _rulesById.Count;

        /// <summary>
        /// Získá všechna registrovaná pravidla.
        /// </summary>
        public IReadOnlyList<IRule<TInput>> AllRules => _rulesById.Values.ToList();

        /// <summary>
        /// Získá všechny registrované typy pravidel.
        /// </summary>
        public IReadOnlyList<IRuleType<TInput>> AllRuleTypes => _ruleTypesById.Values.ToList();

        /// <summary>
        /// Získá všechny registrované skupiny pravidel.
        /// </summary>
        public IReadOnlyList<IRuleGroup<TInput>> AllRuleGroups => _ruleGroupsById.Values.ToList();

        /// <summary>
        /// Získá všechny registrované typy skupin pravidel.
        /// </summary>
        public IReadOnlyList<IRuleGroupType<TInput>> AllRuleGroupTypes => _ruleGroupTypesById.Values.ToList();

        #endregion

        #region Konstruktory

        /// <summary>
        /// Vytvoří novou instanci rozšířeného registru pravidel.
        /// </summary>
        public RuleRegistry()
        {
        }

        /// <summary>
        /// Vytvoří novou instanci rozšířeného registru pravidel s počátečními pravidly.
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

        #endregion

        #region Metody pro správu pravidel (IRuleRegistry)

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
        public IReadOnlyList<IRule<TInput>> GetRulesByType(IRuleType<TInput> ruleType)
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

            // Vyčištění typů pravidel
            _ruleTypesById.Clear();
            _ruleTypesByCode.Clear();
            _ruleTypesByStatus[true].Clear();
            _ruleTypesByStatus[false].Clear();

            // Vyčištění skupin pravidel
            _ruleGroupsById.Clear();
            _ruleGroupsByInternalId.Clear();
            _ruleGroupsByName.Clear();
            _ruleGroupsByType.Clear();
            _ruleGroupsByActiveStatus[true].Clear();
            _ruleGroupsByActiveStatus[false].Clear();

            // Vyčištění typů skupin pravidel
            _ruleGroupTypesById.Clear();
            _ruleGroupTypesByCode.Clear();
            _ruleGroupTypesByStatus[true].Clear();
            _ruleGroupTypesByStatus[false].Clear();
        }

        #endregion

        #region Metody pro správu typů pravidel

        /// <summary>
        /// Registruje typ pravidla v registru.
        /// </summary>
        /// <param name="ruleType">Typ pravidla, který má být registrován.</param>
        /// <returns>True pokud byl typ pravidla úspěšně registrován, jinak false.</returns>
        public bool RegisterRuleType(IRuleType<TInput> ruleType)
        {
            if (ruleType == null)
                throw new ArgumentNullException(nameof(ruleType));

            // Kontrola, zda typ pravidla již existuje
            if (_ruleTypesById.ContainsKey(ruleType.Id) || _ruleTypesByCode.ContainsKey(ruleType.Code))
                return false;

            // Přidání typu pravidla do úložiště
            _ruleTypesById[ruleType.Id] = ruleType;
            _ruleTypesByCode[ruleType.Code] = ruleType;
            _ruleTypesByStatus[ruleType.IsEnabled].Add(ruleType);

            return true;
        }

        /// <summary>
        /// Odregistruje typ pravidla z registru.
        /// </summary>
        /// <param name="ruleTypeId">ID typu pravidla, který má být odregistrován.</param>
        /// <returns>True pokud byl typ pravidla úspěšně odregistrován, jinak false.</returns>
        public bool UnregisterRuleType(int ruleTypeId)
        {
            if (!_ruleTypesById.TryGetValue(ruleTypeId, out var ruleType))
                return false;

            // Kontrola, zda existují pravidla tohoto typu
            if (_rulesByType.ContainsKey(ruleTypeId) && _rulesByType[ruleTypeId].Count > 0)
                throw new InvalidOperationException($"Nelze odregistrovat typ pravidla {ruleType.Code}, protože existují pravidla tohoto typu.");

            // Odstranění typu pravidla z úložiště
            _ruleTypesById.Remove(ruleTypeId);
            _ruleTypesByCode.Remove(ruleType.Code);
            _ruleTypesByStatus[ruleType.IsEnabled].Remove(ruleType);

            return true;
        }

        /// <summary>
        /// Získá typ pravidla podle jeho ID.
        /// </summary>
        /// <param name="ruleTypeId">ID typu pravidla.</param>
        /// <returns>Typ pravidla s daným ID nebo null, pokud typ pravidla neexistuje.</returns>
        public IRuleType<TInput>? GetRuleTypeById(int ruleTypeId)
        {
            _ruleTypesById.TryGetValue(ruleTypeId, out var ruleType);
            return ruleType;
        }

        /// <summary>
        /// Získá typ pravidla podle jeho kódu.
        /// </summary>
        /// <param name="code">Kód typu pravidla.</param>
        /// <returns>Typ pravidla s daným kódem nebo null, pokud typ pravidla neexistuje.</returns>
        public IRuleType<TInput>? GetRuleTypeByCode(string code)
        {
            if (string.IsNullOrEmpty(code))
                throw new ArgumentException("Kód typu pravidla nemůže být prázdný.", nameof(code));

            _ruleTypesByCode.TryGetValue(code, out var ruleType);
            return ruleType;
        }

        /// <summary>
        /// Získá typy pravidel podle jejich aktivního stavu.
        /// </summary>
        /// <param name="isEnabled">Aktivní stav typů pravidel.</param>
        /// <returns>Seznam typů pravidel s daným aktivním stavem.</returns>
        public IReadOnlyList<IRuleType<TInput>> GetRuleTypesByStatus(bool isEnabled)
        {
            return _ruleTypesByStatus[isEnabled];
        }

        /// <summary>
        /// Získá typy pravidel na základě vlastního predikátu.
        /// </summary>
        /// <param name="predicate">Predikát pro filtrování typů pravidel.</param>
        /// <returns>Seznam typů pravidel splňujících daný predikát.</returns>
        public IReadOnlyList<IRuleType<TInput>> GetRuleTypesByPredicate(Func<IRuleType<TInput>, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return _ruleTypesById.Values.Where(predicate).ToList();
        }

        #endregion

        #region Metody pro správu skupin pravidel

        /// <summary>
        /// Registruje skupinu pravidel v registru.
        /// </summary>
        /// <param name="ruleGroup">Skupina pravidel, která má být registrována.</param>
        /// <returns>True pokud byla skupina pravidel úspěšně registrována, jinak false.</returns>
        public bool RegisterRuleGroup(IRuleGroup<TInput> ruleGroup)
        {
            if (ruleGroup == null)
                throw new ArgumentNullException(nameof(ruleGroup));

            // Kontrola, zda skupina pravidel již existuje
            if (_ruleGroupsById.ContainsKey(ruleGroup.GroupId))
                return false;

            // Přidání skupiny pravidel do úložiště
            _ruleGroupsById[ruleGroup.GroupId] = ruleGroup;
            _ruleGroupsByInternalId[ruleGroup.Id] = ruleGroup;

            // Indexace podle názvu
            if (!string.IsNullOrEmpty(ruleGroup.Name))
            {
                if (!_ruleGroupsByName.TryGetValue(ruleGroup.Name, out var nameList))
                {
                    nameList = new List<IRuleGroup<TInput>>();
                    _ruleGroupsByName[ruleGroup.Name] = nameList;
                }
                nameList.Add(ruleGroup);
            }

            // Indexace podle typu
            int typeId = ruleGroup.Type.Id;
            if (!_ruleGroupsByType.TryGetValue(typeId, out var typeList))
            {
                typeList = new List<IRuleGroup<TInput>>();
                _ruleGroupsByType[typeId] = typeList;
            }
            typeList.Add(ruleGroup);

            // Indexace podle aktivního stavu
            _ruleGroupsByActiveStatus[ruleGroup.IsActive].Add(ruleGroup);

            // Automatická registrace všech pravidel ve skupině
            foreach (var rule in ruleGroup.Rules)
            {
                if (!_rulesById.ContainsKey(rule.RuleId))
                {
                    RegisterRule(rule);
                }
            }

            return true;
        }

        /// <summary>
        /// Odregistruje skupinu pravidel z registru.
        /// </summary>
        /// <param name="groupId">ID skupiny pravidel, která má být odregistrována.</param>
        /// <param name="unregisterRules">Zda mají být odregistrována i pravidla ve skupině.</param>
        /// <returns>True pokud byla skupina pravidel úspěšně odregistrována, jinak false.</returns>
        public bool UnregisterRuleGroup(string groupId, bool unregisterRules = false)
        {
            if (string.IsNullOrEmpty(groupId))
                throw new ArgumentException("ID skupiny pravidel nemůže být prázdné.", nameof(groupId));

            if (!_ruleGroupsById.TryGetValue(groupId, out var ruleGroup))
                return false;

            // Odregistrace pravidel ve skupině, pokud je požadována
            if (unregisterRules)
            {
                foreach (var rule in ruleGroup.Rules)
                {
                    UnregisterRule(rule.RuleId);
                }
            }

            // Odstranění skupiny pravidel z úložiště
            _ruleGroupsById.Remove(groupId);
            _ruleGroupsByInternalId.Remove(ruleGroup.Id);

            // Odstranění z indexu podle názvu
            if (!string.IsNullOrEmpty(ruleGroup.Name) && _ruleGroupsByName.TryGetValue(ruleGroup.Name, out var nameList))
            {
                nameList.Remove(ruleGroup);
                if (nameList.Count == 0)
                    _ruleGroupsByName.Remove(ruleGroup.Name);
            }

            // Odstranění z indexu podle typu
            int typeId = ruleGroup.Type.Id;
            if (_ruleGroupsByType.TryGetValue(typeId, out var typeList))
            {
                typeList.Remove(ruleGroup);
                if (typeList.Count == 0)
                    _ruleGroupsByType.Remove(typeId);
            }

            // Odstranění z indexu podle aktivního stavu
            _ruleGroupsByActiveStatus[ruleGroup.IsActive].Remove(ruleGroup);

            return true;
        }

        /// <summary>
        /// Získá skupinu pravidel podle jejího ID.
        /// </summary>
        /// <param name="groupId">ID skupiny pravidel.</param>
        /// <returns>Skupina pravidel s daným ID nebo null, pokud skupina pravidel neexistuje.</returns>
        public IRuleGroup<TInput>? GetRuleGroupById(string groupId)
        {
            if (string.IsNullOrEmpty(groupId))
                throw new ArgumentException("ID skupiny pravidel nemůže být prázdné.", nameof(groupId));

            _ruleGroupsById.TryGetValue(groupId, out var ruleGroup);
            return ruleGroup;
        }

        /// <summary>
        /// Získá skupinu pravidel podle jejího interního ID.
        /// </summary>
        /// <param name="internalId">Interní ID skupiny pravidel.</param>
        /// <returns>Skupina pravidel s daným interním ID nebo null, pokud skupina pravidel neexistuje.</returns>
        public IRuleGroup<TInput>? GetRuleGroupByInternalId(int internalId)
        {
            _ruleGroupsByInternalId.TryGetValue(internalId, out var ruleGroup);
            return ruleGroup;
        }

        /// <summary>
        /// Získá skupiny pravidel podle jejich názvu.
        /// </summary>
        /// <param name="name">Název skupiny pravidel.</param>
        /// <returns>Seznam skupin pravidel s daným názvem nebo prázdný seznam, pokud žádné skupiny pravidel neexistují.</returns>
        public IReadOnlyList<IRuleGroup<TInput>> GetRuleGroupsByName(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Název skupiny pravidel nemůže být prázdný.", nameof(name));

            if (_ruleGroupsByName.TryGetValue(name, out var groups))
                return groups;

            return Array.Empty<IRuleGroup<TInput>>();
        }

        /// <summary>
        /// Získá skupiny pravidel podle typu.
        /// </summary>
        /// <param name="ruleGroupTypeId">ID typu skupiny pravidel.</param>
        /// <returns>Seznam skupin pravidel daného typu nebo prázdný seznam, pokud žádné skupiny pravidel neexistují.</returns>
        public IReadOnlyList<IRuleGroup<TInput>> GetRuleGroupsByType(int ruleGroupTypeId)
        {
            if (_ruleGroupsByType.TryGetValue(ruleGroupTypeId, out var groups))
                return groups;

            return Array.Empty<IRuleGroup<TInput>>();
        }

        /// <summary>
        /// Získá skupiny pravidel podle typu.
        /// </summary>
        /// <param name="ruleGroupType">Typ skupiny pravidel.</param>
        /// <returns>Seznam skupin pravidel daného typu nebo prázdný seznam, pokud žádné skupiny pravidel neexistují.</returns>
        public IReadOnlyList<IRuleGroup<TInput>> GetRuleGroupsByType(IRuleGroupType<TInput> ruleGroupType)
        {
            if (ruleGroupType == null)
                throw new ArgumentNullException(nameof(ruleGroupType));

            return GetRuleGroupsByType(ruleGroupType.Id);
        }

        /// <summary>
        /// Získá skupiny pravidel podle jejich aktivního stavu.
        /// </summary>
        /// <param name="isActive">Aktivní stav skupin pravidel.</param>
        /// <returns>Seznam skupin pravidel s daným aktivním stavem.</returns>
        public IReadOnlyList<IRuleGroup<TInput>> GetRuleGroupsByActiveStatus(bool isActive)
        {
            return _ruleGroupsByActiveStatus[isActive];
        }

        /// <summary>
        /// Získá aktivní skupiny pravidel.
        /// </summary>
        /// <returns>Seznam aktivních skupin pravidel.</returns>
        public IReadOnlyList<IRuleGroup<TInput>> GetActiveRuleGroups()
        {
            return _ruleGroupsByActiveStatus[true];
        }

        /// <summary>
        /// Získá skupiny pravidel na základě vlastního predikátu.
        /// </summary>
        /// <param name="predicate">Predikát pro filtrování skupin pravidel.</param>
        /// <returns>Seznam skupin pravidel splňujících daný predikát.</returns>
        public IReadOnlyList<IRuleGroup<TInput>> GetRuleGroupsByPredicate(Func<IRuleGroup<TInput>, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return _ruleGroupsById.Values.Where(predicate).ToList();
        }

        /// <summary>
        /// Přidá pravidlo do skupiny pravidel.
        /// </summary>
        /// <param name="groupId">ID skupiny pravidel.</param>
        /// <param name="rule">Pravidlo, které má být přidáno.</param>
        /// <returns>True pokud bylo pravidlo úspěšně přidáno, jinak false.</returns>
        public bool AddRuleToGroup(string groupId, IRule<TInput> rule)
        {
            if (string.IsNullOrEmpty(groupId))
                throw new ArgumentException("ID skupiny pravidel nemůže být prázdné.", nameof(groupId));

            if (rule == null)
                throw new ArgumentNullException(nameof(rule));

            if (!_ruleGroupsById.TryGetValue(groupId, out var ruleGroup))
                return false;

            // Registrace pravidla, pokud ještě není registrováno
            if (!_rulesById.ContainsKey(rule.RuleId))
            {
                RegisterRule(rule);
            }

            // Přidání pravidla do skupiny
            ruleGroup.AddRule((dynamic)rule);
            return true;
        }

        /// <summary>
        /// Odebere pravidlo ze skupiny pravidel.
        /// </summary>
        /// <param name="groupId">ID skupiny pravidel.</param>
        /// <param name="ruleId">ID pravidla, které má být odebráno.</param>
        /// <param name="unregisterRule">Zda má být pravidlo odregistrováno z registru.</param>
        /// <returns>True pokud bylo pravidlo úspěšně odebráno, jinak false.</returns>
        public bool RemoveRuleFromGroup(string groupId, string ruleId, bool unregisterRule = false)
        {
            if (string.IsNullOrEmpty(groupId))
                throw new ArgumentException("ID skupiny pravidel nemůže být prázdné.", nameof(groupId));

            if (string.IsNullOrEmpty(ruleId))
                throw new ArgumentException("ID pravidla nemůže být prázdné.", nameof(ruleId));

            if (!_ruleGroupsById.TryGetValue(groupId, out var ruleGroup))
                return false;

            if (!_rulesById.TryGetValue(ruleId, out var rule))
                return false;

            // Odebrání pravidla ze skupiny
            bool removed = ruleGroup.RemoveRule((dynamic)rule);

            // Odregistrace pravidla, pokud je požadováno
            if (removed && unregisterRule)
            {
                UnregisterRule(ruleId);
            }

            return removed;
        }

        #endregion

        #region Metody pro správu typů skupin pravidel

        /// <summary>
        /// Registruje typ skupiny pravidel v registru.
        /// </summary>
        /// <param name="ruleGroupType">Typ skupiny pravidel, který má být registrován.</param>
        /// <returns>True pokud byl typ skupiny pravidel úspěšně registrován, jinak false.</returns>
        public bool RegisterRuleGroupType(IRuleGroupType<TInput> ruleGroupType)
        {
            if (ruleGroupType == null)
                throw new ArgumentNullException(nameof(ruleGroupType));

            // Kontrola, zda typ skupiny pravidel již existuje
            if (_ruleGroupTypesById.ContainsKey(ruleGroupType.Id) || _ruleGroupTypesByCode.ContainsKey(ruleGroupType.Code))
                return false;

            // Přidání typu skupiny pravidel do úložiště
            _ruleGroupTypesById[ruleGroupType.Id] = ruleGroupType;
            _ruleGroupTypesByCode[ruleGroupType.Code] = ruleGroupType;
            _ruleGroupTypesByStatus[ruleGroupType.IsEnabled].Add(ruleGroupType);

            return true;
        }

        /// <summary>
        /// Odregistruje typ skupiny pravidel z registru.
        /// </summary>
        /// <param name="ruleGroupTypeId">ID typu skupiny pravidel, který má být odregistrován.</param>
        /// <returns>True pokud byl typ skupiny pravidel úspěšně odregistrován, jinak false.</returns>
        public bool UnregisterRuleGroupType(int ruleGroupTypeId)
        {
            if (!_ruleGroupTypesById.TryGetValue(ruleGroupTypeId, out var ruleGroupType))
                return false;

            // Kontrola, zda existují skupiny pravidel tohoto typu
            if (_ruleGroupsByType.ContainsKey(ruleGroupTypeId) && _ruleGroupsByType[ruleGroupTypeId].Count > 0)
                throw new InvalidOperationException($"Nelze odregistrovat typ skupiny pravidel {ruleGroupType.Code}, protože existují skupiny pravidel tohoto typu.");

            // Odstranění typu skupiny pravidel z úložiště
            _ruleGroupTypesById.Remove(ruleGroupTypeId);
            _ruleGroupTypesByCode.Remove(ruleGroupType.Code);
            _ruleGroupTypesByStatus[ruleGroupType.IsEnabled].Remove(ruleGroupType);

            return true;
        }

        /// <summary>
        /// Získá typ skupiny pravidel podle jeho ID.
        /// </summary>
        /// <param name="ruleGroupTypeId">ID typu skupiny pravidel.</param>
        /// <returns>Typ skupiny pravidel s daným ID nebo null, pokud typ skupiny pravidel neexistuje.</returns>
        public IRuleGroupType<TInput>? GetRuleGroupTypeById(int ruleGroupTypeId)
        {
            _ruleGroupTypesById.TryGetValue(ruleGroupTypeId, out var ruleGroupType);
            return ruleGroupType;
        }

        /// <summary>
        /// Získá typ skupiny pravidel podle jeho kódu.
        /// </summary>
        /// <param name="code">Kód typu skupiny pravidel.</param>
        /// <returns>Typ skupiny pravidel s daným kódem nebo null, pokud typ skupiny pravidel neexistuje.</returns>
        public IRuleGroupType<TInput>? GetRuleGroupTypeByCode(string code)
        {
            if (string.IsNullOrEmpty(code))
                throw new ArgumentException("Kód typu skupiny pravidel nemůže být prázdný.", nameof(code));

            _ruleGroupTypesByCode.TryGetValue(code, out var ruleGroupType);
            return ruleGroupType;
        }

        /// <summary>
        /// Získá typy skupin pravidel podle jejich aktivního stavu.
        /// </summary>
        /// <param name="isEnabled">Aktivní stav typů skupin pravidel.</param>
        /// <returns>Seznam typů skupin pravidel s daným aktivním stavem.</returns>
        public IReadOnlyList<IRuleGroupType<TInput>> GetRuleGroupTypesByStatus(bool isEnabled)
        {
            return _ruleGroupTypesByStatus[isEnabled];
        }

        /// <summary>
        /// Získá typy skupin pravidel na základě vlastního predikátu.
        /// </summary>
        /// <param name="predicate">Predikát pro filtrování typů skupin pravidel.</param>
        /// <returns>Seznam typů skupin pravidel splňujících daný predikát.</returns>
        public IReadOnlyList<IRuleGroupType<TInput>> GetRuleGroupTypesByPredicate(Func<IRuleGroupType<TInput>, bool> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return _ruleGroupTypesById.Values.Where(predicate).ToList();
        }

        #endregion

        #region Pokročilé metody pro práci s pravidly a skupinami

        /// <summary>
        /// Vyhodnotí, zda pravidlo patří do nějaké registrované skupiny.
        /// </summary>
        /// <param name="ruleId">ID pravidla.</param>
        /// <returns>True pokud pravidlo patří do nějaké skupiny, jinak false.</returns>
        public bool IsRuleInAnyGroup(string ruleId)
        {
            if (string.IsNullOrEmpty(ruleId))
                throw new ArgumentException("ID pravidla nemůže být prázdné.", nameof(ruleId));

            if (!_rulesById.TryGetValue(ruleId, out var rule))
                return false;

            return _ruleGroupsById.Values.Any(group => group.Rules.Any(r => r.RuleId == ruleId));
        }

        /// <summary>
        /// Získá všechny skupiny, které obsahují dané pravidlo.
        /// </summary>
        /// <param name="ruleId">ID pravidla.</param>
        /// <returns>Seznam skupin obsahujících dané pravidlo.</returns>
        public IReadOnlyList<IRuleGroup<TInput>> GetGroupsContainingRule(string ruleId)
        {
            if (string.IsNullOrEmpty(ruleId))
                throw new ArgumentException("ID pravidla nemůže být prázdné.", nameof(ruleId));

            return _ruleGroupsById.Values
                .Where(group => group.Rules.Any(r => r.RuleId == ruleId))
                .ToList();
        }

        /// <summary>
        /// Získá všechna pravidla, která jsou součástí nějaké skupiny.
        /// </summary>
        /// <returns>Seznam pravidel, která jsou součástí nějaké skupiny.</returns>
        public IReadOnlyList<IRule<TInput>> GetRulesInGroups()
        {
            var ruleIds = new HashSet<string>();

            foreach (var group in _ruleGroupsById.Values)
            {
                foreach (var rule in group.Rules)
                {
                    ruleIds.Add(rule.RuleId);
                }
            }

            return ruleIds
                .Select(id => _rulesById[id])
                .ToList();
        }

        /// <summary>
        /// Získá všechna pravidla, která nejsou součástí žádné skupiny.
        /// </summary>
        /// <returns>Seznam pravidel, která nejsou součástí žádné skupiny.</returns>
        public IReadOnlyList<IRule<TInput>> GetRulesNotInGroups()
        {
            var ruleIdsInGroups = new HashSet<string>();

            foreach (var group in _ruleGroupsById.Values)
            {
                foreach (var rule in group.Rules)
                {
                    ruleIdsInGroups.Add(rule.RuleId);
                }
            }

            return _rulesById.Values
                .Where(rule => !ruleIdsInGroups.Contains(rule.RuleId))
                .ToList();
        }

        /// <summary>
        /// Sloučí více skupin pravidel do jedné nové skupiny.
        /// </summary>
        /// <param name="groupIds">ID skupin, které mají být sloučeny.</param>
        /// <param name="newGroupType">Typ nové skupiny.</param>
        /// <param name="newGroupName">Název nové skupiny.</param>
        /// <param name="newGroupDescription">Popis nové skupiny.</param>
        /// <returns>Nová skupina pravidel obsahující všechna pravidla z původních skupin.</returns>
        public IRuleGroup<TInput> MergeGroups(
            IEnumerable<string> groupIds,
            IRuleGroupType<TInput> newGroupType,
            string newGroupName,
            string? newGroupDescription = null)
        {
            if (groupIds == null)
                throw new ArgumentNullException(nameof(groupIds));

            if (newGroupType == null)
                throw new ArgumentNullException(nameof(newGroupType));

            if (string.IsNullOrEmpty(newGroupName))
                throw new ArgumentException("Název nové skupiny nemůže být prázdný.", nameof(newGroupName));

            // Získání skupin, které mají být sloučeny
            var groupsToMerge = new List<IRuleGroup<TInput>>();
            foreach (var groupId in groupIds)
            {
                if (_ruleGroupsById.TryGetValue(groupId, out var group))
                {
                    groupsToMerge.Add(group);
                }
            }

            if (groupsToMerge.Count == 0)
                throw new ArgumentException("Žádná ze zadaných skupin neexistuje.", nameof(groupIds));

            // Vytvoření seznamu všech pravidel z původních skupin
            var allRules = new List<IRule<TInput>>();
            foreach (var group in groupsToMerge)
            {
                allRules.AddRange(group.Rules);
            }

            // Vytvoření nové skupiny s unikátními pravidly
            var uniqueRules = allRules.GroupBy(r => r.RuleId).Select(g => g.First()).Cast<Rule<TInput>>();
            var mergedGroup = new RuleGroup<TInput>(
                GetNextGroupId(),
                newGroupType,
                uniqueRules,
                Guid.NewGuid().ToString(),
                newGroupName,
                newGroupDescription,
                true,
                DateTimeOffset.UtcNow);

            // Registrace nové skupiny
            RegisterRuleGroup(mergedGroup);

            return mergedGroup;
        }

        // Pomocná metoda pro generování nového ID skupiny
        private int GetNextGroupId()
        {
            return _ruleGroupsByInternalId.Count > 0 ? _ruleGroupsByInternalId.Keys.Max() + 1 : 1;
        }

        #endregion
    }
}
using System;
using System.Collections.Generic;
using Ruleflow.NET.Engine.Models.Rule.Interface;
using Ruleflow.NET.Engine.Models.Rule.Type.Interface;

namespace Ruleflow.NET.Engine.Registry.Interface
{
    /// <summary>
    /// Rozhraní pro registr pravidel, který uchovává pravidla v paměti a poskytuje rychlý přístup k nim.
    /// </summary>
    /// <typeparam name="TInput">Typ validovaných dat.</typeparam>
    public interface IRuleRegistry<TInput>
    {
        /// <summary>
        /// Vrátí počet registrovaných pravidel.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Získá všechna registrovaná pravidla.
        /// </summary>
        IReadOnlyList<IRule<TInput>> AllRules { get; }

        /// <summary>
        /// Registruje pravidlo v registru.
        /// </summary>
        /// <param name="rule">Pravidlo, které má být registrováno.</param>
        /// <returns>True pokud bylo pravidlo úspěšně registrováno, jinak false (např. pokud již existuje pravidlo se stejným ID).</returns>
        bool RegisterRule(IRule<TInput> rule);

        /// <summary>
        /// Odregistruje pravidlo z registru.
        /// </summary>
        /// <param name="ruleId">ID pravidla, které má být odregistrováno.</param>
        /// <returns>True pokud bylo pravidlo úspěšně odregistrováno, jinak false (např. pokud pravidlo neexistuje).</returns>
        bool UnregisterRule(string ruleId);

        /// <summary>
        /// Aktualizuje pravidlo v registru.
        /// </summary>
        /// <param name="rule">Aktualizované pravidlo.</param>
        /// <returns>True pokud bylo pravidlo úspěšně aktualizováno, jinak false (např. pokud pravidlo neexistuje).</returns>
        bool UpdateRule(IRule<TInput> rule);

        /// <summary>
        /// Získá pravidlo podle jeho ID.
        /// </summary>
        /// <param name="ruleId">ID pravidla.</param>
        /// <returns>Pravidlo s daným ID nebo null, pokud pravidlo neexistuje.</returns>
        IRule<TInput>? GetRuleById(string ruleId);

        /// <summary>
        /// Získá pravidlo podle jeho interního ID.
        /// </summary>
        /// <param name="internalId">Interní ID pravidla.</param>
        /// <returns>Pravidlo s daným interním ID nebo null, pokud pravidlo neexistuje.</returns>
        IRule<TInput>? GetRuleByInternalId(int internalId);

        /// <summary>
        /// Získá pravidla podle jejich názvu.
        /// </summary>
        /// <param name="name">Název pravidla.</param>
        /// <returns>Seznam pravidel s daným názvem nebo prázdný seznam, pokud žádná pravidla neexistují.</returns>
        IReadOnlyList<IRule<TInput>> GetRulesByName(string name);

        /// <summary>
        /// Získá pravidla podle jejich typu.
        /// </summary>
        /// <param name="ruleTypeId">ID typu pravidla.</param>
        /// <returns>Seznam pravidel daného typu nebo prázdný seznam, pokud žádná pravidla neexistují.</returns>
        IReadOnlyList<IRule<TInput>> GetRulesByType(int ruleTypeId);

        /// <summary>
        /// Získá pravidla podle jejich typu.
        /// </summary>
        /// <param name="ruleType">Typ pravidla.</param>
        /// <returns>Seznam pravidel daného typu nebo prázdný seznam, pokud žádná pravidla neexistují.</returns>
        IReadOnlyList<IRule<TInput>> GetRulesByType(IRuleType ruleType);

        /// <summary>
        /// Získá pravidla podle priority.
        /// </summary>
        /// <param name="priority">Priorita pravidel.</param>
        /// <returns>Seznam pravidel s danou prioritou nebo prázdný seznam, pokud žádná pravidla neexistují.</returns>
        IReadOnlyList<IRule<TInput>> GetRulesByPriority(int priority);

        /// <summary>
        /// Získá pravidla seřazená podle priority (sestupně).
        /// </summary>
        /// <returns>Seznam pravidel seřazených podle priority.</returns>
        IReadOnlyList<IRule<TInput>> GetRulesByPriorityOrder();

        /// <summary>
        /// Získá pravidla podle jejich aktivního stavu.
        /// </summary>
        /// <param name="isActive">Aktivní stav pravidel.</param>
        /// <returns>Seznam pravidel s daným aktivním stavem.</returns>
        IReadOnlyList<IRule<TInput>> GetRulesByActiveStatus(bool isActive);

        /// <summary>
        /// Získá aktivní pravidla.
        /// </summary>
        /// <returns>Seznam aktivních pravidel.</returns>
        IReadOnlyList<IRule<TInput>> GetActiveRules();

        /// <summary>
        /// Získá aktivní pravidla seřazená podle priority (sestupně).
        /// </summary>
        /// <returns>Seznam aktivních pravidel seřazených podle priority.</returns>
        IReadOnlyList<IRule<TInput>> GetActiveRulesByPriorityOrder();

        /// <summary>
        /// Získá pravidla na základě vlastního predikátu.
        /// </summary>
        /// <param name="predicate">Predikát pro filtrování pravidel.</param>
        /// <returns>Seznam pravidel splňujících daný predikát.</returns>
        IReadOnlyList<IRule<TInput>> GetRulesByPredicate(Func<IRule<TInput>, bool> predicate);

        /// <summary>
        /// Vyčistí registr pravidel.
        /// </summary>
        void Clear();
    }
}
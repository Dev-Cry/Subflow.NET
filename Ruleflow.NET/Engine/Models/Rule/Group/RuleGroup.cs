using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ruleflow.NET.Engine.Models.Rule.Group.Interface;
using Ruleflow.NET.Engine.Models.Rule.Interface;

namespace Ruleflow.NET.Engine.Models.Rule.Group
{
    /// <summary>
    /// Skupina validačních pravidel v Ruleflow.NET systému, pevně navázaná na svůj typ.
    /// </summary>
    /// <typeparam name="TInput">Typ dat, která budou validována pravidly ve skupině.</typeparam>
    public class RuleGroup<TInput> : IRuleGroup<TInput>
    {
        public int Id { get; }
        public string GroupId { get; }
        public string? Name { get; }
        public string? Description { get; }
        public bool IsActive { get; }
        public DateTimeOffset Timestamp { get; }

        // silná vazba na RuleGroupType
        public int RuleGroupTypeId { get; }
        public IRuleGroupType<TInput> Type { get; }

        // Implementace IRuleGroupType pro rozhraní
        IRuleGroupType<TInput> IRuleGroup<TInput>.Type => Type;

        // kolekce pravidel ve skupině
        private readonly List<Rule<TInput>> _rules;

        // Explicitní implementace rozhraní pro Rules
        IReadOnlyList<IRule<TInput>> IRuleGroup<TInput>.Rules => _rules.Cast<IRule<TInput>>().ToList();

        // Původní vlastnost pro přístup ke konkrétním implementacím
        public IReadOnlyList<Rule<TInput>> Rules => _rules;

        /// <summary>
        /// Vytvoří novou skupinu pravidel.
        /// </summary>
        /// <param name="id">ID skupiny.</param>
        /// <param name="type">Typ skupiny.</param>
        /// <param name="initialRules">Volitelné počáteční pravidla.</param>
        /// <param name="groupId">Volitelné ID skupiny. Pokud není uvedeno, vygeneruje se GUID.</param>
        /// <param name="name">Volitelný název skupiny.</param>
        /// <param name="description">Volitelný popis skupiny.</param>
        /// <param name="isActive">Zda je skupina aktivní.</param>
        /// <param name="timestamp">Volitelný časový údaj. Pokud není uveden, použije se aktuální čas.</param>
        public RuleGroup(
            int id,
            IRuleGroupType<TInput> type,
            IEnumerable<Rule<TInput>>? initialRules = null,
            string? groupId = null,
            string? name = null,
            string? description = null,
            bool isActive = true,
            DateTimeOffset? timestamp = null)
        {
            Id = id;
            GroupId = groupId ?? Guid.NewGuid().ToString();
            Name = name;
            Description = description;
            IsActive = isActive;
            Timestamp = timestamp?.ToUniversalTime() ?? DateTimeOffset.UtcNow;

            RuleGroupTypeId = type.Id;
            Type = type ?? throw new ArgumentNullException(nameof(type));

            _rules = initialRules != null
                ? new List<Rule<TInput>>(initialRules)
                : new List<Rule<TInput>>();
        }

        /// <summary>
        /// Přidá pravidlo do skupiny.
        /// </summary>
        public void AddRule(Rule<TInput> rule)
        {
            if (rule == null) throw new ArgumentNullException(nameof(rule));
            _rules.Add(rule);
        }

        /// <summary>
        /// Odebere pravidlo ze skupiny.
        /// </summary>
        /// <returns>True pokud bylo pravidlo odebráno, jinak false.</returns>
        public bool RemoveRule(Rule<TInput> rule)
            => _rules.Remove(rule);

        /// <summary>
        /// Implementace rozhraní IRuleGroup - přidá pravidlo do skupiny.
        /// </summary>
        void IRuleGroup<TInput>.AddRule(IRule<TInput> rule)
        {
            if (rule == null) throw new ArgumentNullException(nameof(rule));

            // Pokud je pravidlo konkrétní implementace Rule<TInput>, přidáme ho
            if (rule is Rule<TInput> concreteRule)
            {
                AddRule(concreteRule);
            }
            else
            {
                throw new ArgumentException("Pravidlo musí být typu Rule<TInput>.", nameof(rule));
            }
        }

        /// <summary>
        /// Implementace rozhraní IRuleGroup - odebere pravidlo ze skupiny.
        /// </summary>
        /// <returns>True pokud bylo pravidlo odebráno, jinak false.</returns>
        bool IRuleGroup<TInput>.RemoveRule(IRule<TInput> rule)
        {
            if (rule == null) throw new ArgumentNullException(nameof(rule));

            // Pokud je pravidlo konkrétní implementace Rule<TInput>, odebereme ho
            if (rule is Rule<TInput> concreteRule)
            {
                return RemoveRule(concreteRule);
            }

            return false;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"RuleGroup(Id={Id}, GroupType=\"{Type.Code}\"");
            sb.Append($", GroupId=\"{GroupId}\"");
            if (!string.IsNullOrEmpty(Name))
                sb.Append($", Name=\"{Name}\"");
            if (!string.IsNullOrEmpty(Description))
                sb.Append($", Description=\"{Description}\"");
            sb.Append($", RulesCount={_rules.Count}");
            sb.Append($", IsActive={IsActive}");
            sb.Append($", Timestamp=\"{Timestamp:o}\"");
            sb.Append(")");
            return sb.ToString();
        }
    }
}
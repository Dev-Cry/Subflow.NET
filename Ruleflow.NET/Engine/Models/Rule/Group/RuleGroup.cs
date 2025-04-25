using System.Text;

namespace Ruleflow.NET.Engine.Models.Rule.Group
{

    /// <summary>
    /// Skupina validačních pravidel v Ruleflow.NET systému, pevně navázaná na svůj typ.
    /// </summary>
    /// <typeparam name="TInput">Typ dat, která budou validována pravidly ve skupině.</typeparam>
    public class RuleGroup<TInput>(
        int id,
        RuleGroupType type,
        IEnumerable<Rule<TInput>>? initialRules = null,
        string? groupId = null,
        string? name = null,
        string? description = null,
        bool isActive = true,
        DateTimeOffset? timestamp = null)
    {
        public int Id { get; } = id;
        public string GroupId { get; } = groupId ?? Guid.NewGuid().ToString();
        public string? Name { get; } = name;
        public string? Description { get; } = description;
        public bool IsActive { get; } = isActive;
        public DateTimeOffset Timestamp { get; } = timestamp?.ToUniversalTime() ?? DateTimeOffset.UtcNow;

        // silná vazba na RuleGroupType
        public int RuleGroupTypeId { get; } = type.Id;
        public RuleGroupType Type { get; } = type ?? throw new ArgumentNullException(nameof(type));

        // kolekce pravidel ve skupině
        private readonly List<Rule<TInput>> _rules = initialRules != null
                ? new List<Rule<TInput>>(initialRules)
                : new List<Rule<TInput>>();
        public IReadOnlyList<Rule<TInput>> Rules => _rules;

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
        public bool RemoveRule(Rule<TInput> rule)
            => _rules.Remove(rule);



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
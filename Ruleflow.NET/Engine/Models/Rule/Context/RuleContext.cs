using System.Text;

namespace Ruleflow.NET.Engine.Models.Rule.Context
{

    /// <summary>
    /// Základní kontext pro sdílení pravidla.
    /// Uchovává metadata i uživatelské parametry.
    /// </summary>
    public class RuleContext
    {
        public Guid ContextId { get; } = Guid.NewGuid();
        public string Name { get; }
        public string? Description { get; }
        private readonly Dictionary<string, object> _parameters;
        public IReadOnlyDictionary<string, object> Parameters => _parameters;
        public DateTimeOffset Timestamp { get; } = DateTimeOffset.UtcNow;

        public RuleContext(string name, string? description = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description;
            _parameters = new Dictionary<string, object>();
        }

        /// <summary>
        /// Přidá nebo aktualizuje parametr v kontextu.
        /// </summary>
        public void AddParameter(string key, object value)
            => _parameters[key] = value;

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"RuleContext(Id={ContextId}, Name=\"{Name}\"");
            if (!string.IsNullOrEmpty(Description)) sb.Append($", Description=\"{Description}\"");
            sb.Append($", Params={_parameters.Count}");
            sb.Append($", Timestamp=\"{Timestamp:o}\"");
            sb.Append(")");
            return sb.ToString();
        }
    }

    /// <summary>
    /// Specializovaný kontext, který automaticky přidá metadata z pravidla.
    /// </summary>
    public class RuleContext<TInput> : RuleContext
    {
        public RuleContext(Rule<TInput> rule, string? description = null)
            : base(rule.Name ?? "UnnamedRule", description)
        {
            AddParameter("RuleId", rule.RuleId);
            AddParameter("RulePriority", rule.Priority);
            AddParameter("RuleIsActive", rule.IsActive);
            AddParameter("RuleTimestamp", rule.Timestamp);
        }
    }
}
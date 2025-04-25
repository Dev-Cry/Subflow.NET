using System;
using System.Text;

namespace Ruleflow.NET.Engine.Models.Rule
{
    /// <summary>
    /// Reprezentuje kategorii či typ pravidla v systému Ruleflow.NET.
    /// </summary>
    public class RuleType
    {
        public int Id { get; }
        public string Code { get; }
        public string Name { get; }
        public string? Description { get; }
        public bool IsEnabled { get; }
        public DateTimeOffset CreatedAt { get; }

        public RuleType(
            int id,
            string code,
            string name,
            string? description = null,
            bool isEnabled = true,
            DateTimeOffset? createdAt = null)
        {
            Id = id;
            Code = code ?? throw new ArgumentNullException(nameof(code));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description;
            IsEnabled = isEnabled;
            CreatedAt = createdAt?.ToUniversalTime() ?? DateTimeOffset.UtcNow;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"RuleType(Id={Id}, Code=\"{Code}\", Name=\"{Name}\"");
            if (!string.IsNullOrEmpty(Description))
                sb.Append($", Description=\"{Description}\"");
            sb.Append($", IsEnabled={IsEnabled}");
            sb.Append($", CreatedAt=\"{CreatedAt:o}\"");
            sb.Append(")");
            return sb.ToString();
        }
    }
}


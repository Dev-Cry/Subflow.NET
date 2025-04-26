using Ruleflow.NET.Engine.Models.Rule.Group.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ruleflow.NET.Engine.Models.Rule.Group
{
    /// <summary>
    /// Reprezentuje kategorii či typ skupiny pravidel s typovou vazbou na vstupní data.
    /// </summary>
    /// <typeparam name="TInput">Typ validovaných dat.</typeparam>
    public class RuleGroupType<TInput> : IRuleGroupType<TInput>
    {
        public int Id { get; }
        public string Code { get; }
        public string Name { get; }
        public string? Description { get; }
        public bool IsEnabled { get; }
        public DateTimeOffset CreatedAt { get; }

        /// <summary>
        /// Vytvoří nový typ skupiny pravidel.
        /// </summary>
        /// <param name="id">Jedinečný identifikátor.</param>
        /// <param name="code">Kód typu skupiny.</param>
        /// <param name="name">Název typu skupiny.</param>
        /// <param name="description">Volitelný popis.</param>
        /// <param name="isEnabled">Zda je typ povolen.</param>
        /// <param name="createdAt">Datum a čas vytvoření.</param>
        public RuleGroupType(
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
            sb.Append($"RuleGroupType<{typeof(TInput).Name}>(Id={Id}, Code=\"{Code}\", Name=\"{Name}\"");
            if (!string.IsNullOrEmpty(Description))
                sb.Append($", Description=\"{Description}\"");
            sb.Append($", IsEnabled={IsEnabled}");
            sb.Append($", CreatedAt=\"{CreatedAt:o}\"");
            sb.Append(")");
            return sb.ToString();
        }
    }
}
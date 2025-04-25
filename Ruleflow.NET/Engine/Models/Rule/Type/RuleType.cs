using Ruleflow.NET.Engine.Models.Rule.Type.Interface;
using System;
using System.Text;

namespace Ruleflow.NET.Engine.Models.Rule.Type
{
    /// <summary>
    /// Reprezentuje kategorii či typ pravidla v systému Ruleflow.NET.
    /// </summary>
    public class RuleType : IRuleType
    {
        /// <summary>
        /// Jedinečný identifikátor typu pravidla.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Kód typu pravidla (technický identifikátor).
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Název typu pravidla (čitelný pro člověka).
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Volitelný popis typu pravidla.
        /// </summary>
        public string? Description { get; }

        /// <summary>
        /// Indikuje, zda je tento typ pravidla povolen v systému.
        /// </summary>
        public bool IsEnabled { get; }

        /// <summary>
        /// Datum a čas vytvoření typu pravidla.
        /// </summary>
        public DateTimeOffset CreatedAt { get; }

        /// <summary>
        /// Vytvoří nový typ pravidla.
        /// </summary>
        /// <param name="id">Jedinečný identifikátor.</param>
        /// <param name="code">Kód typu pravidla.</param>
        /// <param name="name">Název typu pravidla.</param>
        /// <param name="description">Volitelný popis.</param>
        /// <param name="isEnabled">Zda je typ povolen.</param>
        /// <param name="createdAt">Datum a čas vytvoření.</param>
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
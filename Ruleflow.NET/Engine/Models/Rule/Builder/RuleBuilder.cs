using Ruleflow.NET.Engine.Models.Rule.Builder.Interface;
using Ruleflow.NET.Engine.Models.Rule.Type;

namespace Ruleflow.NET.Engine.Models.Rule.Builder
{
    /// <summary>
    /// Základní třída pro buildery pravidel.
    /// </summary>
    /// <typeparam name="TInput">Typ validovaných dat.</typeparam>
    /// <typeparam name="TBuilder">Konkrétní typ builderu (pro metodu fluent řetězení).</typeparam>
    public abstract class RuleBuilder<TInput, TBuilder> : IRuleBuilder<TInput, TBuilder>
        where TBuilder : RuleBuilder<TInput, TBuilder>
    {
        // Společné vlastnosti
        protected int Id { get; set; }
        protected string? RuleId { get; set; }
        protected string? Name { get; set; }
        protected string? Description { get; set; }
        protected int Priority { get; set; }
        protected bool IsActive { get; set; } = true;
        protected DateTimeOffset? Timestamp { get; set; }
        protected RuleType Type { get; set; }

        protected RuleBuilder(int id, RuleType type)
        {
            Id = id;
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        /// <summary>
        /// Nastaví jedinečný identifikátor pravidla.
        /// </summary>
        /// <param name="ruleId">Identifikátor pravidla.</param>
        /// <returns>Instance builderu pro řetězení.</returns>
        public TBuilder WithRuleId(string ruleId)
        {
            RuleId = ruleId;
            return (TBuilder)this;
        }

        /// <summary>
        /// Nastaví název pravidla.
        /// </summary>
        /// <param name="name">Název pravidla.</param>
        /// <returns>Instance builderu pro řetězení.</returns>
        public TBuilder WithName(string name)
        {
            Name = name;
            return (TBuilder)this;
        }

        /// <summary>
        /// Nastaví popis pravidla.
        /// </summary>
        /// <param name="description">Popis pravidla.</param>
        /// <returns>Instance builderu pro řetězení.</returns>
        public TBuilder WithDescription(string description)
        {
            Description = description;
            return (TBuilder)this;
        }

        /// <summary>
        /// Nastaví prioritu pravidla.
        /// </summary>
        /// <param name="priority">Priorita pravidla.</param>
        /// <returns>Instance builderu pro řetězení.</returns>
        public TBuilder WithPriority(int priority)
        {
            Priority = priority;
            return (TBuilder)this;
        }

        /// <summary>
        /// Nastaví, zda je pravidlo aktivní.
        /// </summary>
        /// <param name="isActive">Aktivita pravidla.</param>
        /// <returns>Instance builderu pro řetězení.</returns>
        public TBuilder SetActive(bool isActive)
        {
            IsActive = isActive;
            return (TBuilder)this;
        }

        /// <summary>
        /// Nastaví časovou značku pravidla.
        /// </summary>
        /// <param name="timestamp">Časová značka.</param>
        /// <returns>Instance builderu pro řetězení.</returns>
        public TBuilder WithTimestamp(DateTimeOffset timestamp)
        {
            Timestamp = timestamp;
            return (TBuilder)this;
        }

        /// <summary>
        /// Nastaví typ pravidla.
        /// </summary>
        /// <param name="type">Typ pravidla.</param>
        /// <returns>Instance builderu pro řetězení.</returns>
        public TBuilder WithType(RuleType type)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            return (TBuilder)this;
        }

        /// <summary>
        /// Sestaví a vrátí pravidlo podle nastavené konfigurace.
        /// </summary>
        /// <returns>Vytvořené pravidlo.</returns>
        public abstract Rule<TInput> Build();
    }
}
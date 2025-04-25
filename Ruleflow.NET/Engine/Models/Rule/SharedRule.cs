using System.Text;

namespace Ruleflow.NET.Engine.Models.Rule
{

    /// <summary>
    /// Spojuje instanci pravidla s konkrétním kontextem pro audit či sdílení.
    /// </summary>
    public class SharedRule<TInput>
    {
        public int Id { get; }
        public Rule<TInput> Rule { get; }
        public RuleContext Context { get; }
        public DateTimeOffset SharedAt { get; }

        private SharedRule(int id, Rule<TInput> rule, RuleContext context, DateTimeOffset sharedAt)
        {
            Id = id;
            Rule = rule;
            Context = context;
            SharedAt = sharedAt;
        }

        /// <summary>
        /// Vytvoří SharedRule a implicitně sestaví specializovaný kontext s metadaty pravidla.
        /// </summary>
        public static SharedRule<TInput> Create(
            int id,
            Rule<TInput> rule,
            string? contextDescription = null)
        {
            var ctx = new RuleContext<TInput>(rule, contextDescription);
            return new SharedRule<TInput>(id, rule, ctx, DateTimeOffset.UtcNow);
        }

        public override string ToString()
        {
            return new StringBuilder()
                .Append($"SharedRule(Id={Id}, Rule={Rule.RuleId}")
                .Append($", Priority={Rule.Priority}")
                .Append($", IsActive={Rule.IsActive}")
                .Append($", Context={Context.ContextId}")
                .Append($", SharedAt=\"{SharedAt:o}\"")
                .Append(")")
                .ToString();
        }
    }
}
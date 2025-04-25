using Ruleflow.NET.Engine.Models.Rule;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ruleflow.NET.Engine.Models.Evaluation
{
    /// <summary>
    /// Reprezentuje výsledek vyhodnocení pravidla proti vstupním datům.
    /// </summary>
    /// <typeparam name="TInput">Typ validovaných vstupních dat.</typeparam>
    public class RuleEvaluationResult<TInput>
    {
        /// <summary>
        /// Pravidlo, které bylo vyhodnoceno.
        /// </summary>
        public Rule<TInput> Rule { get; }

        /// <summary>
        /// Kontext použitý při vyhodnocení, obsahuje metadata pravidla i uživatelské parametry.
        /// </summary>
        public RuleContext Context { get; }

        /// <summary>
        /// Vstupní data, proti kterým bylo pravidlo aplikováno.
        /// </summary>
        public TInput Input { get; }

        /// <summary>
        /// Indikuje, zda pravidlo prošlo (true) nebo neprošlo (false).
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Seznam zpráv a chyb vzniklých při vyhodnocení pravidla.
        /// </summary>
        public IReadOnlyList<string> Messages { get; }

        /// <summary>
        /// Čas, kdy bylo vyhodnocení provedeno.
        /// </summary>
        public DateTimeOffset EvaluatedAt { get; }

        /// <summary>
        /// Vytvoří instanci výsledku vyhodnocení pravidla.
        /// </summary>
        /// <param name="rule">Pravidlo, které se vyhodnocuje.</param>
        /// <param name="context">Kontext vyhodnocení.</param>
        /// <param name="input">Vstupní data pro vyhodnocení.</param>
        /// <param name="isSuccess">Výsledek vyhodnocení.</param>
        /// <param name="messages">Volitelné zprávy či chyby.</param>
        /// <param name="evaluatedAt">Čas vyhodnocení. Implicitně aktuální UTC čas.</param>
        public RuleEvaluationResult(
            Rule<TInput> rule,
            RuleContext context,
            TInput input,
            bool isSuccess,
            IEnumerable<string>? messages = null,
            DateTimeOffset? evaluatedAt = null)
        {
            Rule = rule ?? throw new ArgumentNullException(nameof(rule));
            Context = context ?? throw new ArgumentNullException(nameof(context));
            Input = input;
            IsSuccess = isSuccess;
            Messages = new List<string>(messages ?? Array.Empty<string>());
            EvaluatedAt = evaluatedAt?.ToUniversalTime() ?? DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// Vytvoří instanci označující úspěšné vyhodnocení bez zpráv.
        /// </summary>
        public static RuleEvaluationResult<TInput> Success(
            Rule<TInput> rule,
            RuleContext context,
            TInput input)
        {
            return new RuleEvaluationResult<TInput>(rule, context, input, true, null);
        }

        /// <summary>
        /// Vytvoří instanci označující neúspěšné vyhodnocení s chybovými zprávami.
        /// </summary>
        public static RuleEvaluationResult<TInput> Failure(
            Rule<TInput> rule,
            RuleContext context,
            TInput input,
            IEnumerable<string> messages)
        {
            if (messages == null) throw new ArgumentNullException(nameof(messages));
            return new RuleEvaluationResult<TInput>(rule, context, input, false, messages);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"RuleEvaluationResult(RuleId=\"{Rule.RuleId}\", Success={IsSuccess}");
            sb.Append($", MessagesCount={Messages.Count}");
            sb.Append($", EvaluatedAt=\"{EvaluatedAt:o}\"");
            sb.Append(")");
            return sb.ToString();
        }
    }
}

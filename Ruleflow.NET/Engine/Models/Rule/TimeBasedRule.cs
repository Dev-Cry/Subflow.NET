using Ruleflow.NET.Engine.Models.Evaluation;
using Ruleflow.NET.Engine.Models.Rule.Context;
using Ruleflow.NET.Engine.Models.Rule.Interface;
using Ruleflow.NET.Engine.Models.Rule.Type;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ruleflow.NET.Engine.Models.Rule
{
    /// <summary>
    /// Implementace časového pravidla, které vyhodnocuje podmínky na základě času.
    /// </summary>
    /// <typeparam name="TInput">Typ validovaných dat.</typeparam>
    public class TimeBasedRule<TInput> : Rule<TInput>, ITimeBasedRule<TInput>
    {
        /// <summary>
        /// Delegát pro časovou podmínku.
        /// </summary>
        /// <param name="input">Vstupní data pro validaci.</param>
        /// <param name="evaluationTime">Čas, pro který se podmínka vyhodnocuje.</param>
        /// <param name="context">Kontext vyhodnocení pravidla.</param>
        /// <returns>True pokud časová podmínka platí, jinak false.</returns>
        public delegate bool TimeConditionDelegate(TInput input, DateTimeOffset evaluationTime, RuleContext context);

        private readonly TimeConditionDelegate _timeConditionFunc;

        /// <summary>
        /// Indikuje, zda se má pro vyhodnocení použít aktuální čas.
        /// </summary>
        public bool UseCurrentTime { get; }

        /// <summary>
        /// Vytvoří nové časové pravidlo.
        /// </summary>
        /// <param name="id">Identifikátor pravidla.</param>
        /// <param name="type">Typ pravidla.</param>
        /// <param name="timeConditionFunc">Funkce časové podmínky.</param>
        /// <param name="useCurrentTime">Zda se má pro vyhodnocení použít aktuální čas.</param>
        /// <param name="ruleId">Volitelný jedinečný identifikátor pravidla (GUID).</param>
        /// <param name="name">Název pravidla.</param>
        /// <param name="description">Popis pravidla.</param>
        /// <param name="priority">Priorita pravidla.</param>
        /// <param name="isActive">Zda je pravidlo aktivní.</param>
        /// <param name="timestamp">Časová značka vytvoření pravidla.</param>
        public TimeBasedRule(
            int id,
            RuleType type,
            TimeConditionDelegate timeConditionFunc,
            bool useCurrentTime = true,
            string? ruleId = null,
            string? name = null,
            string? description = null,
            int priority = 0,
            bool isActive = true,
            DateTimeOffset? timestamp = null)
            : base(id, type, ruleId, name, description, priority, isActive, timestamp)
        {
            _timeConditionFunc = timeConditionFunc ?? throw new ArgumentNullException(nameof(timeConditionFunc));
            UseCurrentTime = useCurrentTime;
        }

        /// <summary>
        /// Vyhodnotí, zda časová podmínka platí pro zadaný čas.
        /// </summary>
        /// <param name="input">Vstupní data pro validaci.</param>
        /// <param name="evaluationTime">Čas, pro který se podmínka vyhodnocuje.</param>
        /// <param name="context">Kontext vyhodnocení pravidla.</param>
        /// <returns>True pokud časová podmínka platí, jinak false.</returns>
        public bool EvaluateTimeCondition(TInput input, DateTimeOffset evaluationTime, RuleContext context)
        {
            return _timeConditionFunc(input, evaluationTime, context);
        }

        /// <summary>
        /// Vyhodnotí pravidlo proti zadaným vstupním datům.
        /// </summary>
        /// <param name="input">Vstupní data pro validaci.</param>
        /// <param name="context">Kontext vyhodnocení pravidla.</param>
        /// <returns>Výsledek vyhodnocení pravidla.</returns>
        public RuleEvaluationResult<TInput> Evaluate(TInput input, RuleContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            // Pokud není pravidlo aktivní, vrátíme úspěch (neaktivní pravidla se přeskakují)
            if (!IsActive)
                return RuleEvaluationResult<TInput>.Success(this, context, input);

            try
            {
                // Získáme čas pro vyhodnocení - buď současný, nebo z kontextu
                DateTimeOffset evaluationTime;

                if (UseCurrentTime)
                {
                    evaluationTime = DateTimeOffset.UtcNow;
                }
                else
                {
                    if (context.Parameters.TryGetValue("EvaluationTime", out var timeParam) &&
                        timeParam is DateTimeOffset time)
                    {
                        evaluationTime = time;
                    }
                    else
                    {
                        return RuleEvaluationResult<TInput>.Failure(this, context, input,
                            new[] { $"Časové pravidlo {Name ?? RuleId} vyžaduje parametr 'EvaluationTime' v kontextu." });
                    }
                }

                // Přidáme použitý čas vyhodnocení do kontextu pro debugging a audit
                var timeContext = new RuleContext(context.Name, context.Description);
                foreach (var param in context.Parameters)
                {
                    timeContext.AddParameter(param.Key, param.Value);
                }
                timeContext.AddParameter("ActualEvaluationTime", evaluationTime);

                bool result = EvaluateTimeCondition(input, evaluationTime, timeContext);

                if (result)
                    return RuleEvaluationResult<TInput>.Success(this, timeContext, input);
                else
                    return RuleEvaluationResult<TInput>.Failure(this, timeContext, input,
                        new[] { $"Časové pravidlo {Name ?? RuleId} nesplněno v čase {evaluationTime:o}." });
            }
            catch (Exception ex)
            {
                return RuleEvaluationResult<TInput>.Failure(this, context, input,
                    new[] { $"Chyba při vyhodnocení časového pravidla {Name ?? RuleId}: {ex.Message}" });
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder(base.ToString());
            sb.Remove(sb.Length - 1, 1); // odstranění posledního znaku ")"
            sb.Append(", Type=TimeBasedRule");
            sb.Append($", UseCurrentTime={UseCurrentTime}");
            sb.Append(")");
            return sb.ToString();
        }
    }
}
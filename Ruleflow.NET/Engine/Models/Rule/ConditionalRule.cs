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
    /// Implementace podmínkového pravidla, které volí různé větve na základě podmínky.
    /// </summary>
    /// <typeparam name="TInput">Typ validovaných dat.</typeparam>
    public class ConditionalRule<TInput> : Rule<TInput>, IConditionalRule<TInput>
    {
        /// <summary>
        /// Delegát pro vyhodnocení podmínky.
        /// </summary>
        /// <param name="input">Vstupní data pro validaci.</param>
        /// <param name="context">Kontext vyhodnocení pravidla.</param>
        /// <returns>True pro Then větev, False pro Else větev.</returns>
        public delegate bool ConditionDelegate(TInput input, RuleContext context);

        private readonly ConditionDelegate _conditionFunc;

        /// <summary>
        /// Pravidlo, které se vyhodnotí při splnění podmínky.
        /// </summary>
        public IRule<TInput>? ThenRule { get; set; }

        /// <summary>
        /// Pravidlo, které se vyhodnotí při nesplnění podmínky.
        /// </summary>
        public IRule<TInput>? ElseRule { get; set; }

        /// <summary>
        /// Vytvoří nové podmínkové pravidlo.
        /// </summary>
        /// <param name="id">Identifikátor pravidla.</param>
        /// <param name="type">Typ pravidla.</param>
        /// <param name="conditionFunc">Funkce podmínky.</param>
        /// <param name="thenRule">Pravidlo pro THEN větev.</param>
        /// <param name="elseRule">Pravidlo pro ELSE větev.</param>
        /// <param name="ruleId">Volitelný jedinečný identifikátor pravidla (GUID).</param>
        /// <param name="name">Název pravidla.</param>
        /// <param name="description">Popis pravidla.</param>
        /// <param name="priority">Priorita pravidla.</param>
        /// <param name="isActive">Zda je pravidlo aktivní.</param>
        /// <param name="timestamp">Časová značka vytvoření pravidla.</param>
        public ConditionalRule(
            int id,
            RuleType type,
            ConditionDelegate conditionFunc,
            IRule<TInput>? thenRule = null,
            IRule<TInput>? elseRule = null,
            string? ruleId = null,
            string? name = null,
            string? description = null,
            int priority = 0,
            bool isActive = true,
            DateTimeOffset? timestamp = null)
            : base(id, type, ruleId, name, description, priority, isActive, timestamp)
        {
            _conditionFunc = conditionFunc ?? throw new ArgumentNullException(nameof(conditionFunc));
            ThenRule = thenRule;
            ElseRule = elseRule;
        }

        /// <summary>
        /// Vyhodnotí podmínku pravidla.
        /// </summary>
        /// <param name="input">Vstupní data pro validaci.</param>
        /// <param name="context">Kontext vyhodnocení pravidla.</param>
        /// <returns>True pro Then větev, False pro Else větev.</returns>
        public bool EvaluateCondition(TInput input, RuleContext context)
        {
            return _conditionFunc(input, context);
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
                bool conditionResult = EvaluateCondition(input, context);

                // Přidáme výsledek podmínky do kontextu, aby byl dostupný pro vnořená pravidla
                var conditionContext = new RuleContext(context.Name, context.Description);
                foreach (var param in context.Parameters)
                {
                    conditionContext.AddParameter(param.Key, param.Value);
                }
                conditionContext.AddParameter("ConditionResult", conditionResult);

                if (conditionResult)
                {
                    if (ThenRule != null)
                        return ThenRule.Evaluate(input, conditionContext);
                    else
                        return RuleEvaluationResult<TInput>.Success(this, context, input);
                }
                else
                {
                    if (ElseRule != null)
                        return ElseRule.Evaluate(input, conditionContext);
                    else
                        return RuleEvaluationResult<TInput>.Success(this, context, input);
                }
            }
            catch (Exception ex)
            {
                return RuleEvaluationResult<TInput>.Failure(this, context, input,
                    new[] { $"Chyba při vyhodnocení podmínkového pravidla {Name ?? RuleId}: {ex.Message}" });
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder(base.ToString());
            sb.Remove(sb.Length - 1, 1); // odstranění posledního znaku ")"
            sb.Append(", Type=ConditionalRule");
            sb.Append(ThenRule != null ? ", HasThenRule=true" : ", HasThenRule=false");
            sb.Append(ElseRule != null ? ", HasElseRule=true" : ", HasElseRule=false");
            sb.Append(")");
            return sb.ToString();
        }
    }
}
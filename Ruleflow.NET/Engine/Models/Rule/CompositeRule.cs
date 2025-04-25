using Ruleflow.NET.Engine.Models.Evaluation;
using Ruleflow.NET.Engine.Models.Rule.Context;
using Ruleflow.NET.Engine.Models.Rule.Interface;
using Ruleflow.NET.Engine.Models.Rule.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ruleflow.NET.Engine.Models.Rule
{
    /// <summary>
    /// Implementace kompozitního pravidla, které sdružuje více pravidel.
    /// </summary>
    /// <typeparam name="TInput">Typ validovaných dat.</typeparam>
    public class CompositeRule<TInput> : Rule<TInput>, ICompositeRule<TInput>
    {
        /// <summary>
        /// Delegát pro vyhodnocení výsledků dětských pravidel.
        /// </summary>
        /// <param name="input">Vstupní data pro validaci.</param>
        /// <param name="context">Kontext vyhodnocení pravidla.</param>
        /// <param name="results">Výsledky vyhodnocení dětských pravidel.</param>
        /// <returns>True pokud kompozitní pravidlo prošlo, jinak false.</returns>
        public delegate bool EvaluateChildrenDelegate(TInput input, RuleContext context, IReadOnlyList<RuleEvaluationResult<TInput>> results);

        private readonly List<IRule<TInput>> _childRules = new List<IRule<TInput>>();
        private readonly EvaluateChildrenDelegate _evaluateFunc;

        /// <summary>
        /// Seznam vnořených pravidel.
        /// </summary>
        public IReadOnlyList<IRule<TInput>> ChildRules => _childRules;

        /// <summary>
        /// Vytvoří nové kompozitní pravidlo.
        /// </summary>
        /// <param name="id">Identifikátor pravidla.</param>
        /// <param name="type">Typ pravidla.</param>
        /// <param name="evaluateFunc">Funkce pro vyhodnocení výsledků dětských pravidel.</param>
        /// <param name="initialRules">Volitelný seznam počátečních vnořených pravidel.</param>
        /// <param name="ruleId">Volitelný jedinečný identifikátor pravidla (GUID).</param>
        /// <param name="name">Název pravidla.</param>
        /// <param name="description">Popis pravidla.</param>
        /// <param name="priority">Priorita pravidla.</param>
        /// <param name="isActive">Zda je pravidlo aktivní.</param>
        /// <param name="timestamp">Časová značka vytvoření pravidla.</param>
        public CompositeRule(
            int id,
            RuleType type,
            EvaluateChildrenDelegate evaluateFunc,
            IEnumerable<IRule<TInput>>? initialRules = null,
            string? ruleId = null,
            string? name = null,
            string? description = null,
            int priority = 0,
            bool isActive = true,
            DateTimeOffset? timestamp = null)
            : base(id, type, ruleId, name, description, priority, isActive, timestamp)
        {
            _evaluateFunc = evaluateFunc ?? throw new ArgumentNullException(nameof(evaluateFunc));

            if (initialRules != null)
            {
                foreach (var rule in initialRules)
                {
                    AddRule(rule);
                }
            }
        }

        /// <summary>
        /// Přidá pravidlo do kompozitu.
        /// </summary>
        /// <param name="rule">Pravidlo, které bude přidáno do kompozitu.</param>
        public void AddRule(IRule<TInput> rule)
        {
            if (rule == null)
                throw new ArgumentNullException(nameof(rule));

            _childRules.Add(rule);
        }

        /// <summary>
        /// Odebere pravidlo z kompozitu.
        /// </summary>
        /// <param name="rule">Pravidlo, které bude odebráno z kompozitu.</param>
        /// <returns>True, pokud bylo pravidlo úspěšně odebráno, jinak false.</returns>
        public bool RemoveRule(IRule<TInput> rule)
        {
            return _childRules.Remove(rule);
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

            // Nejprve vyhodnotíme všechna dětská pravidla
            var childResults = new List<RuleEvaluationResult<TInput>>();

            foreach (var childRule in _childRules)
            {
                var result = childRule.Evaluate(input, context);
                childResults.Add(result);
            }

            try
            {
                // Přidáme informaci o počtu pravidel a počtu úspěšných/neúspěšných pravidel do kontextu
                var compositeContext = new RuleContext(context.Name, context.Description);
                foreach (var param in context.Parameters)
                {
                    compositeContext.AddParameter(param.Key, param.Value);
                }

                int totalRules = childResults.Count;
                int successRules = childResults.Count(r => r.IsSuccess);
                int failedRules = totalRules - successRules;

                compositeContext.AddParameter("TotalChildRules", totalRules);
                compositeContext.AddParameter("SuccessfulChildRules", successRules);
                compositeContext.AddParameter("FailedChildRules", failedRules);

                bool result = _evaluateFunc(input, compositeContext, childResults);

                if (result)
                    return RuleEvaluationResult<TInput>.Success(this, context, input);
                else
                {
                    // Sbíráme zprávy z neúspěšných dětských pravidel
                    var messages = new List<string> { $"Kompozitní pravidlo {Name ?? RuleId} nebylo splněno." };

                    foreach (var childResult in childResults.Where(r => !r.IsSuccess))
                    {
                        foreach (var message in childResult.Messages)
                        {
                            messages.Add($"- Vnořené pravidlo {childResult.Rule.Name ?? childResult.Rule.RuleId}: {message}");
                        }
                    }

                    return RuleEvaluationResult<TInput>.Failure(this, context, input, messages);
                }
            }
            catch (Exception ex)
            {
                return RuleEvaluationResult<TInput>.Failure(this, context, input,
                    new[] { $"Chyba při vyhodnocení kompozitního pravidla {Name ?? RuleId}: {ex.Message}" });
            }
        }

        /// <summary>
        /// Vytvoří kompozitní pravidlo, které vyžaduje úspěšné vyhodnocení všech vnořených pravidel (AND).
        /// </summary>
        public static CompositeRule<TInput> CreateAnd(
            int id,
            RuleType type,
            IEnumerable<IRule<TInput>>? initialRules = null,
            string? ruleId = null,
            string? name = null,
            string? description = null,
            int priority = 0,
            bool isActive = true,
            DateTimeOffset? timestamp = null)
        {
            return new CompositeRule<TInput>(
                id,
                type,
                (input, context, results) => results.All(r => r.IsSuccess),
                initialRules,
                ruleId,
                name ?? "AND Composite Rule",
                description ?? "Vyžaduje úspěšné vyhodnocení všech vnořených pravidel",
                priority,
                isActive,
                timestamp);
        }

        /// <summary>
        /// Vytvoří kompozitní pravidlo, které vyžaduje úspěšné vyhodnocení alespoň jednoho vnořeného pravidla (OR).
        /// </summary>
        public static CompositeRule<TInput> CreateOr(
            int id,
            RuleType type,
            IEnumerable<IRule<TInput>>? initialRules = null,
            string? ruleId = null,
            string? name = null,
            string? description = null,
            int priority = 0,
            bool isActive = true,
            DateTimeOffset? timestamp = null)
        {
            return new CompositeRule<TInput>(
                id,
                type,
                (input, context, results) => results.Any(r => r.IsSuccess),
                initialRules,
                ruleId,
                name ?? "OR Composite Rule",
                description ?? "Vyžaduje úspěšné vyhodnocení alespoň jednoho vnořeného pravidla",
                priority,
                isActive,
                timestamp);
        }

        /// <summary>
        /// Vytvoří kompozitní pravidlo, které vyžaduje minimální počet úspěšných vnořených pravidel.
        /// </summary>
        public static CompositeRule<TInput> CreateMinimum(
            int id,
            RuleType type,
            int minimumCount,
            IEnumerable<IRule<TInput>>? initialRules = null,
            string? ruleId = null,
            string? name = null,
            string? description = null,
            int priority = 0,
            bool isActive = true,
            DateTimeOffset? timestamp = null)
        {
            return new CompositeRule<TInput>(
                id,
                type,
                (input, context, results) => results.Count(r => r.IsSuccess) >= minimumCount,
                initialRules,
                ruleId,
                name ?? $"Minimum {minimumCount} Composite Rule",
                description ?? $"Vyžaduje úspěšné vyhodnocení alespoň {minimumCount} vnořených pravidel",
                priority,
                isActive,
                timestamp);
        }

        /// <summary>
        /// Vytvoří kompozitní pravidlo, které vyžaduje procentuální úspěšnost vnořených pravidel.
        /// </summary>
        public static CompositeRule<TInput> CreatePercentage(
            int id,
            RuleType type,
            double minimumPercentage,
            IEnumerable<IRule<TInput>>? initialRules = null,
            string? ruleId = null,
            string? name = null,
            string? description = null,
            int priority = 0,
            bool isActive = true,
            DateTimeOffset? timestamp = null)
        {
            return new CompositeRule<TInput>(
                id,
                type,
                (input, context, results) =>
                {
                    if (results.Count == 0) return true; // Pokud nejsou žádná pravidla, považujeme to za úspěch
                    double successRate = (double)results.Count(r => r.IsSuccess) / results.Count * 100;
                    return successRate >= minimumPercentage;
                },
                initialRules,
                ruleId,
                name ?? $"Percentage {minimumPercentage}% Composite Rule",
                description ?? $"Vyžaduje úspěšné vyhodnocení alespoň {minimumPercentage}% vnořených pravidel",
                priority,
                isActive,
                timestamp);
        }

        public override string ToString()
        {
            var sb = new StringBuilder(base.ToString());
            sb.Remove(sb.Length - 1, 1); // odstranění posledního znaku ")"
            sb.Append($", Type=CompositeRule, ChildRulesCount={_childRules.Count})");
            return sb.ToString();
        }
    }
}
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
    /// Implementace pravidla závislého na výsledcích jiných pravidel.
    /// </summary>
    /// <typeparam name="TInput">Typ validovaných dat.</typeparam>
    public class DependentRule<TInput> : Rule<TInput>, IDependentRule<TInput>
    {
        /// <summary>
        /// Delegát pro vyhodnocení závislého pravidla na základě výsledků podpravidel.
        /// </summary>
        /// <param name="input">Vstupní data pro validaci.</param>
        /// <param name="context">Kontext vyhodnocení pravidla.</param>
        /// <param name="results">Výsledky vyhodnocení závislých pravidel.</param>
        /// <returns>True pokud pravidlo prošlo, jinak false.</returns>
        public delegate bool EvaluateDependenciesDelegate(TInput input, RuleContext context, IReadOnlyList<RuleEvaluationResult<TInput>> results);

        private readonly List<IRule<TInput>> _dependencies = new List<IRule<TInput>>();
        private readonly EvaluateDependenciesDelegate _evaluateFunc;

        /// <summary>
        /// Seznam pravidel, na kterých závisí toto pravidlo.
        /// </summary>
        public IReadOnlyList<IRule<TInput>> Dependencies => _dependencies;

        /// <summary>
        /// Vytvoří nové závislé pravidlo.
        /// </summary>
        /// <param name="id">Identifikátor pravidla.</param>
        /// <param name="type">Typ pravidla.</param>
        /// <param name="evaluateFunc">Funkce pro vyhodnocení výsledků závislých pravidel.</param>
        /// <param name="initialDependencies">Volitelný seznam počátečních závislostí.</param>
        /// <param name="ruleId">Volitelný jedinečný identifikátor pravidla (GUID).</param>
        /// <param name="name">Název pravidla.</param>
        /// <param name="description">Popis pravidla.</param>
        /// <param name="priority">Priorita pravidla.</param>
        /// <param name="isActive">Zda je pravidlo aktivní.</param>
        /// <param name="timestamp">Časová značka vytvoření pravidla.</param>
        public DependentRule(
            int id,
            RuleType type,
            EvaluateDependenciesDelegate evaluateFunc,
            IEnumerable<IRule<TInput>>? initialDependencies = null,
            string? ruleId = null,
            string? name = null,
            string? description = null,
            int priority = 0,
            bool isActive = true,
            DateTimeOffset? timestamp = null)
            : base(id, type, ruleId, name, description, priority, isActive, timestamp)
        {
            _evaluateFunc = evaluateFunc ?? throw new ArgumentNullException(nameof(evaluateFunc));

            if (initialDependencies != null)
            {
                foreach (var dependency in initialDependencies)
                {
                    AddDependency(dependency);
                }
            }
        }

        /// <summary>
        /// Přidá závislost na jiném pravidle.
        /// </summary>
        /// <param name="rule">Pravidlo, na kterém závisí toto pravidlo.</param>
        public void AddDependency(IRule<TInput> rule)
        {
            if (rule == null)
                throw new ArgumentNullException(nameof(rule));

            _dependencies.Add(rule);
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

            // Nejprve vyhodnotíme všechna závislá pravidla
            var dependencyResults = new List<RuleEvaluationResult<TInput>>();

            foreach (var dependency in _dependencies)
            {
                var result = dependency.Evaluate(input, context);
                dependencyResults.Add(result);
            }

            try
            {
                bool result = _evaluateFunc(input, context, dependencyResults);

                if (result)
                    return RuleEvaluationResult<TInput>.Success(this, context, input);
                else
                {
                    // Sbíráme zprávy z neúspěšných závislých pravidel
                    var messages = new List<string> { $"Pravidlo {Name ?? RuleId} nebylo splněno." };

                    foreach (var depResult in dependencyResults.Where(r => !r.IsSuccess))
                    {
                        foreach (var message in depResult.Messages)
                        {
                            messages.Add($"- Závislost {depResult.Rule.Name ?? depResult.Rule.RuleId}: {message}");
                        }
                    }

                    return RuleEvaluationResult<TInput>.Failure(this, context, input, messages);
                }
            }
            catch (Exception ex)
            {
                return RuleEvaluationResult<TInput>.Failure(this, context, input,
                    new[] { $"Chyba při vyhodnocení pravidla {Name ?? RuleId}: {ex.Message}" });
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder(base.ToString());
            sb.Remove(sb.Length - 1, 1); // odstranění posledního znaku ")"
            sb.Append($", Type=DependentRule, Dependencies={_dependencies.Count})");
            return sb.ToString();
        }
    }
}
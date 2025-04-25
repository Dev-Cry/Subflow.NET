using Ruleflow.NET.Engine.Models.Rule;
using Ruleflow.NET.Engine.Models.Rule.Builder.Interface;
using Ruleflow.NET.Engine.Models.Rule.Interface;
using Ruleflow.NET.Engine.Models.Rule.Type;
using System.Collections.Generic;

namespace Ruleflow.NET.Engine.Models.Rule.Builder
{
    /// <summary>
    /// Builder pro vytváření pravidel závislých na jiných pravidlech.
    /// </summary>
    /// <typeparam name="TInput">Typ validovaných dat.</typeparam>
    public class DependentRuleBuilder<TInput> : RuleBuilder<TInput, DependentRuleBuilder<TInput>>
    {
        private DependentRule<TInput>.EvaluateDependenciesDelegate? _evaluateFunc;
        private readonly List<IRule<TInput>> _dependencies = new List<IRule<TInput>>();

        /// <summary>
        /// Vytvoří nový builder pro závislé pravidlo.
        /// </summary>
        /// <param name="id">ID pravidla.</param>
        /// <param name="type">Typ pravidla.</param>
        public DependentRuleBuilder(int id, RuleType type) : base(id, type)
        {
        }

        /// <summary>
        /// Nastaví funkci pro vyhodnocení závislostí.
        /// </summary>
        /// <param name="evaluateFunc">Funkce pro vyhodnocení závislostí.</param>
        /// <returns>Instance builderu pro řetězení.</returns>
        public DependentRuleBuilder<TInput> WithEvaluationFunction(DependentRule<TInput>.EvaluateDependenciesDelegate evaluateFunc)
        {
            _evaluateFunc = evaluateFunc;
            return this;
        }

        /// <summary>
        /// Přidá závislost na jiném pravidle.
        /// </summary>
        /// <param name="rule">Pravidlo, na kterém závisí toto pravidlo.</param>
        /// <returns>Instance builderu pro řetězení.</returns>
        public DependentRuleBuilder<TInput> AddDependency(IRule<TInput> rule)
        {
            _dependencies.Add(rule);
            return this;
        }

        /// <summary>
        /// Přidá kolekci závislostí na jiných pravidlech.
        /// </summary>
        /// <param name="rules">Kolekce pravidel, na kterých závisí toto pravidlo.</param>
        /// <returns>Instance builderu pro řetězení.</returns>
        public DependentRuleBuilder<TInput> AddDependencies(IEnumerable<IRule<TInput>> rules)
        {
            foreach (var rule in rules)
            {
                _dependencies.Add(rule);
            }
            return this;
        }

        /// <summary>
        /// Přepne builder do režimu ALL (všechny závislosti musí projít).
        /// </summary>
        /// <returns>Instance builderu pro řetězení.</returns>
        public DependentRuleBuilder<TInput> RequireAllDependencies()
        {
            _evaluateFunc = (input, context, results) => results.All(r => r.IsSuccess);
            return this;
        }

        /// <summary>
        /// Přepne builder do režimu ANY (alespoň jedna závislost musí projít).
        /// </summary>
        /// <returns>Instance builderu pro řetězení.</returns>
        public DependentRuleBuilder<TInput> RequireAnyDependency()
        {
            _evaluateFunc = (input, context, results) => results.Any(r => r.IsSuccess);
            return this;
        }

        /// <summary>
        /// Přepne builder do režimu minimálního počtu úspěšných závislostí.
        /// </summary>
        /// <param name="minimumCount">Minimální počet úspěšných závislostí.</param>
        /// <returns>Instance builderu pro řetězení.</returns>
        public DependentRuleBuilder<TInput> RequireMinimumDependencies(int minimumCount)
        {
            _evaluateFunc = (input, context, results) => results.Count(r => r.IsSuccess) >= minimumCount;
            return this;
        }

        /// <summary>
        /// Sestaví pravidlo podle nastavené konfigurace.
        /// </summary>
        /// <returns>Vytvořené pravidlo.</returns>
        public override Rule<TInput> Build()
        {
            if (_evaluateFunc == null)
                throw new InvalidOperationException("Evaluační funkce musí být nastavena. Použijte metody RequireAllDependencies, RequireAnyDependency, RequireMinimumDependencies nebo WithEvaluationFunction.");

            var rule = new DependentRule<TInput>(
                Id,
                Type,
                _evaluateFunc,
                _dependencies,
                RuleId,
                Name ?? "Unnamed Dependent Rule",
                Description,
                Priority,
                IsActive,
                Timestamp);

            return rule;
        }
    }
}
using Ruleflow.NET.Engine.Models.Rule.Implementation;
using Ruleflow.NET.Engine.Models.Rule.Builder.Interface;
using Ruleflow.NET.Engine.Models.Rule.Type;
using System.Collections.Generic;
using Ruleflow.NET.Engine.Models.Rule;

namespace Ruleflow.NET.Engine.Models.Rule.Builder
{
    /// <summary>
    /// Builder pro vytváření prediktivních pravidel.
    /// </summary>
    /// <typeparam name="TInput">Typ validovaných dat.</typeparam>
    /// <typeparam name="THistoryData">Typ historických dat používaných pro predikci.</typeparam>
    public class PredictiveRuleBuilder<TInput, THistoryData> : RuleBuilder<TInput, PredictiveRuleBuilder<TInput, THistoryData>>
    {
        private PredictiveRule<TInput, THistoryData>.PredictDelegate? _predictFunc;
        private readonly List<THistoryData> _historicalData = new List<THistoryData>();

        /// <summary>
        /// Vytvoří nový builder pro prediktivní pravidlo.
        /// </summary>
        /// <param name="id">ID pravidla.</param>
        /// <param name="type">Typ pravidla.</param>
        public PredictiveRuleBuilder(int id, RuleType type) : base(id, type)
        {
        }

        /// <summary>
        /// Nastaví prediktivní funkci pravidla.
        /// </summary>
        /// <param name="predictFunc">Funkce pro predikci.</param>
        /// <returns>Instance builderu pro řetězení.</returns>
        public PredictiveRuleBuilder<TInput, THistoryData> WithPredictionFunction(PredictiveRule<TInput, THistoryData>.PredictDelegate predictFunc)
        {
            _predictFunc = predictFunc;
            return this;
        }

        /// <summary>
        /// Přidá historická data pro predikci.
        /// </summary>
        /// <param name="data">Historická data.</param>
        /// <returns>Instance builderu pro řetězení.</returns>
        public PredictiveRuleBuilder<TInput, THistoryData> AddHistoricalData(THistoryData data)
        {
            _historicalData.Add(data);
            return this;
        }

        /// <summary>
        /// Přidá kolekci historických dat pro predikci.
        /// </summary>
        /// <param name="data">Kolekce historických dat.</param>
        /// <returns>Instance builderu pro řetězení.</returns>
        public PredictiveRuleBuilder<TInput, THistoryData> AddHistoricalDataRange(IEnumerable<THistoryData> data)
        {
            foreach (var item in data)
            {
                _historicalData.Add(item);
            }
            return this;
        }

        /// <summary>
        /// Sestaví pravidlo podle nastavené konfigurace.
        /// </summary>
        /// <returns>Vytvořené pravidlo.</returns>
        public override Rule<TInput> Build()
        {
            if (_predictFunc == null)
                throw new InvalidOperationException("Prediktivní funkce musí být nastavena.");

            var rule = new PredictiveRule<TInput, THistoryData>(
                Id,
                Type,
                _predictFunc,
                _historicalData,
                RuleId,
                Name ?? "Unnamed Predictive Rule",
                Description,
                Priority,
                IsActive,
                Timestamp);

            return rule;
        }
    }
}
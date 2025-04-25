using Ruleflow.NET.Engine.Models.Evaluation;
using Ruleflow.NET.Engine.Models.Rule.Context;
using Ruleflow.NET.Engine.Models.Rule.Interface;
using Ruleflow.NET.Engine.Models.Rule.Type;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ruleflow.NET.Engine.Models.Rule.Implementation
{
    /// <summary>
    /// Implementace prediktivního pravidla, které využívá historická data pro predikci.
    /// </summary>
    /// <typeparam name="TInput">Typ validovaných dat.</typeparam>
    /// <typeparam name="THistoryData">Typ historických dat používaných pro predikci.</typeparam>
    public class PredictiveRule<TInput, THistoryData> : Rule<TInput>, IPredictiveRule<TInput, THistoryData>
    {
        /// <summary>
        /// Delegát pro prediktivní funkci.
        /// </summary>
        /// <param name="input">Vstupní data pro validaci.</param>
        /// <param name="historicalData">Historická data pro predikci.</param>
        /// <param name="context">Kontext vyhodnocení pravidla.</param>
        /// <returns>True pokud predikce prošla, jinak false.</returns>
        public delegate bool PredictDelegate(TInput input, IReadOnlyList<THistoryData> historicalData, RuleContext context);

        private readonly PredictDelegate _predictFunc;
        private readonly List<THistoryData> _historicalData = new List<THistoryData>();

        /// <summary>
        /// Historická data používaná pro predikci.
        /// </summary>
        public IReadOnlyList<THistoryData> HistoricalData => _historicalData;

        /// <summary>
        /// Vytvoří nové prediktivní pravidlo.
        /// </summary>
        /// <param name="id">Identifikátor pravidla.</param>
        /// <param name="type">Typ pravidla.</param>
        /// <param name="predictFunc">Prediktivní funkce.</param>
        /// <param name="initialHistoricalData">Volitelná počáteční historická data.</param>
        /// <param name="ruleId">Volitelný jedinečný identifikátor pravidla (GUID).</param>
        /// <param name="name">Název pravidla.</param>
        /// <param name="description">Popis pravidla.</param>
        /// <param name="priority">Priorita pravidla.</param>
        /// <param name="isActive">Zda je pravidlo aktivní.</param>
        /// <param name="timestamp">Časová značka vytvoření pravidla.</param>
        public PredictiveRule(
            int id,
            RuleType type,
            PredictDelegate predictFunc,
            IEnumerable<THistoryData>? initialHistoricalData = null,
            string? ruleId = null,
            string? name = null,
            string? description = null,
            int priority = 0,
            bool isActive = true,
            DateTimeOffset? timestamp = null)
            : base(id, type, ruleId, name, description, priority, isActive, timestamp)
        {
            _predictFunc = predictFunc ?? throw new ArgumentNullException(nameof(predictFunc));

            if (initialHistoricalData != null)
            {
                AddHistoricalDataRange(initialHistoricalData);
            }
        }

        /// <summary>
        /// Přidá historická data pro predikci.
        /// </summary>
        /// <param name="data">Historická data.</param>
        public void AddHistoricalData(THistoryData data)
        {
            _historicalData.Add(data);
        }

        /// <summary>
        /// Přidá kolekci historických dat pro predikci.
        /// </summary>
        /// <param name="data">Kolekce historických dat.</param>
        public void AddHistoricalDataRange(IEnumerable<THistoryData> data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            _historicalData.AddRange(data);
        }

        /// <summary>
        /// Vyčistí všechna historická data.
        /// </summary>
        public void ClearHistoricalData()
        {
            _historicalData.Clear();
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
                // Přidáme do kontextu počet historických dat pro debugging
                var predictiveContext = new RuleContext(context.Name, context.Description);
                foreach (var param in context.Parameters)
                {
                    predictiveContext.AddParameter(param.Key, param.Value);
                }
                predictiveContext.AddParameter("HistoricalDataCount", _historicalData.Count);

                bool result = _predictFunc(input, _historicalData, predictiveContext);

                if (result)
                    return RuleEvaluationResult<TInput>.Success(this, context, input);
                else
                    return RuleEvaluationResult<TInput>.Failure(this, context, input,
                        new[] { $"Prediktivní pravidlo {Name ?? RuleId} nebylo splněno." });
            }
            catch (Exception ex)
            {
                return RuleEvaluationResult<TInput>.Failure(this, context, input,
                    new[] { $"Chyba při vyhodnocení prediktivního pravidla {Name ?? RuleId}: {ex.Message}" });
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder(base.ToString());
            sb.Remove(sb.Length - 1, 1); // odstranění posledního znaku ")"
            sb.Append($", Type=PredictiveRule, HistoricalDataCount={_historicalData.Count})");
            return sb.ToString();
        }
    }
}
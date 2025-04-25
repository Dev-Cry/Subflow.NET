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
    /// Implementace jednoduchého pravidla s jednou odpovědností.
    /// </summary>
    /// <typeparam name="TInput">Typ validovaných dat.</typeparam>
    public class SingleResponsibilityRule<TInput> : Rule<TInput>, ISingleResponsibilityRule<TInput>
    {
        /// <summary>
        /// Delegát pro validační funkci.
        /// </summary>
        /// <param name="input">Vstupní data pro validaci.</param>
        /// <param name="context">Kontext vyhodnocení pravidla.</param>
        /// <returns>True pokud validace prošla, jinak false.</returns>
        public delegate bool ValidateDelegate(TInput input, RuleContext context);

        /// <summary>
        /// Funkce implementující validační logiku.
        /// </summary>
        private readonly ValidateDelegate _validateFunc;

        /// <summary>
        /// Vytvoří nové pravidlo s jednou odpovědností.
        /// </summary>
        /// <param name="id">Identifikátor pravidla.</param>
        /// <param name="type">Typ pravidla.</param>
        /// <param name="validateFunc">Validační funkce.</param>
        /// <param name="ruleId">Volitelný jedinečný identifikátor pravidla (GUID).</param>
        /// <param name="name">Název pravidla.</param>
        /// <param name="description">Popis pravidla.</param>
        /// <param name="priority">Priorita pravidla.</param>
        /// <param name="isActive">Zda je pravidlo aktivní.</param>
        /// <param name="timestamp">Časová značka vytvoření pravidla.</param>
        public SingleResponsibilityRule(
            int id,
            RuleType type,
            ValidateDelegate validateFunc,
            string? ruleId = null,
            string? name = null,
            string? description = null,
            int priority = 0,
            bool isActive = true,
            DateTimeOffset? timestamp = null)
            : base(id, type, ruleId, name, description, priority, isActive, timestamp)
        {
            _validateFunc = validateFunc ?? throw new ArgumentNullException(nameof(validateFunc));
        }

        /// <summary>
        /// Implementace validační funkce.
        /// </summary>
        /// <param name="input">Vstupní data pro validaci.</param>
        /// <param name="context">Kontext vyhodnocení pravidla.</param>
        /// <returns>True pokud validace prošla, jinak false.</returns>
        public bool Validate(TInput input, RuleContext context)
        {
            return _validateFunc(input, context);
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
                bool result = Validate(input, context);

                if (result)
                    return RuleEvaluationResult<TInput>.Success(this, context, input);
                else
                    return RuleEvaluationResult<TInput>.Failure(this, context, input, new[] { $"Pravidlo {Name ?? RuleId} nebylo splněno." });
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
            sb.Append(", Type=SingleResponsibilityRule)");
            return sb.ToString();
        }
    }
}
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
    /// Implementace přepínacího pravidla, které vybírá jednu z mnoha možností.
    /// </summary>
    /// <typeparam name="TInput">Typ validovaných dat.</typeparam>
    public class SwitchRule<TInput> : Rule<TInput>, ISwitchRule<TInput>
    {
        /// <summary>
        /// Delegát pro získání přepínacího klíče.
        /// </summary>
        /// <param name="input">Vstupní data pro validaci.</param>
        /// <param name="context">Kontext vyhodnocení pravidla.</param>
        /// <returns>Klíč pro výběr případu.</returns>
        public delegate object SwitchKeyDelegate(TInput input, RuleContext context);

        private readonly SwitchKeyDelegate _switchKeyFunc;
        private readonly Dictionary<object, IRule<TInput>> _cases = new Dictionary<object, IRule<TInput>>();

        /// <summary>
        /// Slovník případů s klíčem a odpovídajícím pravidlem.
        /// </summary>
        public IReadOnlyDictionary<object, IRule<TInput>> Cases => _cases;

        /// <summary>
        /// Výchozí pravidlo, které se použije, pokud žádný případ nevyhovuje.
        /// </summary>
        public IRule<TInput>? DefaultCase { get; set; }

        /// <summary>
        /// Vytvoří nové přepínací pravidlo.
        /// </summary>
        /// <param name="id">Identifikátor pravidla.</param>
        /// <param name="type">Typ pravidla.</param>
        /// <param name="switchKeyFunc">Funkce pro získání přepínacího klíče.</param>
        /// <param name="defaultCase">Výchozí pravidlo.</param>
        /// <param name="initialCases">Volitelný slovník počátečních případů.</param>
        /// <param name="ruleId">Volitelný jedinečný identifikátor pravidla (GUID).</param>
        /// <param name="name">Název pravidla.</param>
        /// <param name="description">Popis pravidla.</param>
        /// <param name="priority">Priorita pravidla.</param>
        /// <param name="isActive">Zda je pravidlo aktivní.</param>
        /// <param name="timestamp">Časová značka vytvoření pravidla.</param>
        public SwitchRule(
            int id,
            RuleType type,
            SwitchKeyDelegate switchKeyFunc,
            IRule<TInput>? defaultCase = null,
            IDictionary<object, IRule<TInput>>? initialCases = null,
            string? ruleId = null,
            string? name = null,
            string? description = null,
            int priority = 0,
            bool isActive = true,
            DateTimeOffset? timestamp = null)
            : base(id, type, ruleId, name, description, priority, isActive, timestamp)
        {
            _switchKeyFunc = switchKeyFunc ?? throw new ArgumentNullException(nameof(switchKeyFunc));
            DefaultCase = defaultCase;

            if (initialCases != null)
            {
                foreach (var kv in initialCases)
                {
                    AddCase(kv.Key, kv.Value);
                }
            }
        }

        /// <summary>
        /// Určí klíč pro přepínání mezi případy.
        /// </summary>
        /// <param name="input">Vstupní data pro validaci.</param>
        /// <param name="context">Kontext vyhodnocení pravidla.</param>
        /// <returns>Klíč pro výběr případu.</returns>
        public object GetSwitchKey(TInput input, RuleContext context)
        {
            return _switchKeyFunc(input, context);
        }

        /// <summary>
        /// Přidá případ do přepínače.
        /// </summary>
        /// <param name="key">Klíč případu.</param>
        /// <param name="rule">Pravidlo odpovídající případu.</param>
        public void AddCase(object key, IRule<TInput> rule)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (rule == null)
                throw new ArgumentNullException(nameof(rule));

            _cases[key] = rule;
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
                // Získáme klíč pro výběr případu
                var key = GetSwitchKey(input, context);

                // Vytvoříme nový kontext, který obsahuje klíč
                var switchContext = new RuleContext(context.Name, context.Description);
                foreach (var param in context.Parameters)
                {
                    switchContext.AddParameter(param.Key, param.Value);
                }
                switchContext.AddParameter("SwitchKey", key);

                // Hledáme odpovídající pravidlo
                if (_cases.TryGetValue(key, out var rule))
                {
                    return rule.Evaluate(input, switchContext);
                }
                else if (DefaultCase != null)
                {
                    return DefaultCase.Evaluate(input, switchContext);
                }
                else
                {
                    // Pokud nemáme výchozí případ a klíč nebyl nalezen, považujeme pravidlo za splněné
                    return RuleEvaluationResult<TInput>.Success(this, context, input);
                }
            }
            catch (Exception ex)
            {
                return RuleEvaluationResult<TInput>.Failure(this, context, input,
                    new[] { $"Chyba při vyhodnocení přepínacího pravidla {Name ?? RuleId}: {ex.Message}" });
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder(base.ToString());
            sb.Remove(sb.Length - 1, 1); // odstranění posledního znaku ")"
            sb.Append($", Type=SwitchRule, CasesCount={_cases.Count}");
            sb.Append(DefaultCase != null ? ", HasDefaultCase=true" : ", HasDefaultCase=false");
            sb.Append(")");
            return sb.ToString();
        }
    }
}
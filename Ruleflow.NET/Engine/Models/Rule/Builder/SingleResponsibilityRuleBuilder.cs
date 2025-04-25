using Ruleflow.NET.Engine.Models.Rule;
using Ruleflow.NET.Engine.Models.Rule.Builder.Interface;
using Ruleflow.NET.Engine.Models.Rule.Type;

namespace Ruleflow.NET.Engine.Models.Rule.Builder
{
    /// <summary>
    /// Builder pro vytváření pravidel s jednou odpovědností.
    /// </summary>
    /// <typeparam name="TInput">Typ validovaných dat.</typeparam>
    public class SingleResponsibilityRuleBuilder<TInput> : RuleBuilder<TInput, SingleResponsibilityRuleBuilder<TInput>>
    {
        private SingleResponsibilityRule<TInput>.ValidateDelegate? _validateFunc;
        private string? _errorMessage;

        /// <summary>
        /// Vytvoří nový builder pro jednoduché pravidlo.
        /// </summary>
        /// <param name="id">ID pravidla.</param>
        /// <param name="type">Typ pravidla.</param>
        public SingleResponsibilityRuleBuilder(int id, RuleType type) : base(id, type)
        {
        }

        /// <summary>
        /// Nastaví validační funkci pravidla.
        /// </summary>
        /// <param name="validateFunc">Funkce pro validaci.</param>
        /// <returns>Instance builderu pro řetězení.</returns>
        public SingleResponsibilityRuleBuilder<TInput> WithValidation(SingleResponsibilityRule<TInput>.ValidateDelegate validateFunc)
        {
            _validateFunc = validateFunc;
            return this;
        }

        /// <summary>
        /// Nastaví chybovou zprávu pro případ neúspěchu validace.
        /// </summary>
        /// <param name="errorMessage">Chybová zpráva.</param>
        /// <returns>Instance builderu pro řetězení.</returns>
        public SingleResponsibilityRuleBuilder<TInput> WithErrorMessage(string errorMessage)
        {
            _errorMessage = errorMessage;
            return this;
        }

        /// <summary>
        /// Sestaví pravidlo podle nastavené konfigurace.
        /// </summary>
        /// <returns>Vytvořené pravidlo.</returns>
        public override Rule<TInput> Build()
        {
            if (_validateFunc == null)
                throw new InvalidOperationException("Validační funkce musí být nastavena.");

            var rule = new SingleResponsibilityRule<TInput>(
                Id,
                Type,
                _validateFunc,
                RuleId,
                Name ?? "Unnamed Single Responsibility Rule",
                Description,
                Priority,
                IsActive,
                Timestamp);

            return rule;
        }
    }
}
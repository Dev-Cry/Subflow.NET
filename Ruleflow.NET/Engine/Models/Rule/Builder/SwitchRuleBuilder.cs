using Ruleflow.NET.Engine.Models.Rule;
using Ruleflow.NET.Engine.Models.Rule.Builder.Interface;
using Ruleflow.NET.Engine.Models.Rule.Interface;
using Ruleflow.NET.Engine.Models.Rule.Type;
using System.Collections.Generic;

namespace Ruleflow.NET.Engine.Models.Rule.Builder
{
    /// <summary>
    /// Builder pro vytváření přepínacích pravidel.
    /// </summary>
    /// <typeparam name="TInput">Typ validovaných dat.</typeparam>
    public class SwitchRuleBuilder<TInput> : RuleBuilder<TInput, SwitchRuleBuilder<TInput>>
    {
        private SwitchRule<TInput>.SwitchKeyDelegate? _switchKeyFunc;
        private readonly Dictionary<object, IRule<TInput>> _cases = new Dictionary<object, IRule<TInput>>();
        private IRule<TInput>? _defaultCase;

        /// <summary>
        /// Vytvoří nový builder pro přepínací pravidlo.
        /// </summary>
        /// <param name="id">ID pravidla.</param>
        /// <param name="type">Typ pravidla.</param>
        public SwitchRuleBuilder(int id, RuleType type) : base(id, type)
        {
        }

        /// <summary>
        /// Nastaví funkci pro určení přepínacího klíče.
        /// </summary>
        /// <param name="switchKeyFunc">Funkce pro získání přepínacího klíče.</param>
        /// <returns>Instance builderu pro řetězení.</returns>
        public SwitchRuleBuilder<TInput> WithSwitchKeyFunction(SwitchRule<TInput>.SwitchKeyDelegate switchKeyFunc)
        {
            _switchKeyFunc = switchKeyFunc;
            return this;
        }

        /// <summary>
        /// Přidá případ do přepínače.
        /// </summary>
        /// <param name="key">Klíč případu.</param>
        /// <param name="rule">Pravidlo odpovídající případu.</param>
        /// <returns>Instance builderu pro řetězení.</returns>
        public SwitchRuleBuilder<TInput> AddCase(object key, IRule<TInput> rule)
        {
            _cases[key] = rule;
            return this;
        }

        /// <summary>
        /// Nastaví výchozí pravidlo, které se použije, pokud žádný případ nevyhovuje.
        /// </summary>
        /// <param name="defaultRule">Výchozí pravidlo.</param>
        /// <returns>Instance builderu pro řetězení.</returns>
        public SwitchRuleBuilder<TInput> WithDefaultCase(IRule<TInput> defaultRule)
        {
            _defaultCase = defaultRule;
            return this;
        }

        /// <summary>
        /// Sestaví pravidlo podle nastavené konfigurace.
        /// </summary>
        /// <returns>Vytvořené pravidlo.</returns>
        public override Rule<TInput> Build()
        {
            if (_switchKeyFunc == null)
                throw new InvalidOperationException("Funkce pro určení přepínacího klíče musí být nastavena.");

            var rule = new SwitchRule<TInput>(
                Id,
                Type,
                _switchKeyFunc,
                _defaultCase,
                _cases,
                RuleId,
                Name ?? "Unnamed Switch Rule",
                Description,
                Priority,
                IsActive,
                Timestamp);

            return rule;
        }
    }

    /// <summary>
    /// Specializovaný builder pro přepínací pravidlo se silně typovaným klíčem.
    /// </summary>
    /// <typeparam name="TInput">Typ validovaných dat.</typeparam>
    /// <typeparam name="TKey">Typ klíče pro přepínání.</typeparam>
    public class SwitchRuleBuilder<TInput, TKey> : RuleBuilder<TInput, SwitchRuleBuilder<TInput, TKey>>
    {
        private Func<TInput, Rule.Context.RuleContext, TKey>? _switchKeyFunc;
        private readonly Dictionary<TKey, IRule<TInput>> _cases = new Dictionary<TKey, IRule<TInput>>();
        private IRule<TInput>? _defaultCase;

        /// <summary>
        /// Vytvoří nový builder pro přepínací pravidlo se silně typovaným klíčem.
        /// </summary>
        /// <param name="id">ID pravidla.</param>
        /// <param name="type">Typ pravidla.</param>
        public SwitchRuleBuilder(int id, RuleType type) : base(id, type)
        {
        }

        /// <summary>
        /// Nastaví funkci pro určení přepínacího klíče.
        /// </summary>
        /// <param name="switchKeyFunc">Funkce pro získání přepínacího klíče.</param>
        /// <returns>Instance builderu pro řetězení.</returns>
        public SwitchRuleBuilder<TInput, TKey> WithSwitchKeyFunction(Func<TInput, Rule.Context.RuleContext, TKey> switchKeyFunc)
        {
            _switchKeyFunc = switchKeyFunc;
            return this;
        }

        /// <summary>
        /// Přidá případ do přepínače.
        /// </summary>
        /// <param name="key">Klíč případu.</param>
        /// <param name="rule">Pravidlo odpovídající případu.</param>
        /// <returns>Instance builderu pro řetězení.</returns>
        public SwitchRuleBuilder<TInput, TKey> AddCase(TKey key, IRule<TInput> rule)
        {
            _cases[key] = rule;
            return this;
        }

        /// <summary>
        /// Nastaví výchozí pravidlo, které se použije, pokud žádný případ nevyhovuje.
        /// </summary>
        /// <param name="defaultRule">Výchozí pravidlo.</param>
        /// <returns>Instance builderu pro řetězení.</returns>
        public SwitchRuleBuilder<TInput, TKey> WithDefaultCase(IRule<TInput> defaultRule)
        {
            _defaultCase = defaultRule;
            return this;
        }

        /// <summary>
        /// Sestaví pravidlo podle nastavené konfigurace.
        /// </summary>
        /// <returns>Vytvořené pravidlo.</returns>
        public override Rule<TInput> Build()
        {
            if (_switchKeyFunc == null)
                throw new InvalidOperationException("Funkce pro určení přepínacího klíče musí být nastavena.");

            // Převod na netypovaný slovník
            var cases = _cases.ToDictionary<KeyValuePair<TKey, IRule<TInput>>, object, IRule<TInput>>(
                pair => pair.Key as object ?? throw new InvalidOperationException("Klíč nemůže být null."),
                pair => pair.Value);

            // Převod na netypovanou switch funkci
            SwitchRule<TInput>.SwitchKeyDelegate switchKeyFunc = (input, context) => _switchKeyFunc(input, context) as object
                ?? throw new InvalidOperationException("Návratová hodnota přepínací funkce nemůže být null.");

            var rule = new SwitchRule<TInput>(
                Id,
                Type,
                switchKeyFunc,
                _defaultCase,
                cases,
                RuleId,
                Name ?? "Unnamed Switch Rule",
                Description,
                Priority,
                IsActive,
                Timestamp);

            return rule;
        }
    }
}
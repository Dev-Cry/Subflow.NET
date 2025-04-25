using Ruleflow.NET.Engine.Models.Rule;
using Ruleflow.NET.Engine.Models.Rule.Context;
using Ruleflow.NET.Engine.Models.Rule.Implementation;
using Ruleflow.NET.Engine.Models.Rule.Type;
using Ruleflow.NET.Engine.Models.Rule.Type.Interface;
using System;
using System.Collections.Generic;

namespace Ruleflow.NET.Engine.Factory.Interface
{
    /// <summary>
    /// Rozhraní továrny pro vytváření pravidel.
    /// </summary>
    public interface IRuleFactory
    {
        /// <summary>
        /// Získá výchozí typ pravidla.
        /// </summary>
        /// <returns>Výchozí typ pravidla.</returns>
        IRuleType GetDefaultRuleType();

        /// <summary>
        /// Nastaví výchozí typ pravidla.
        /// </summary>
        /// <param name="ruleType">Výchozí typ pravidla.</param>
        void SetDefaultRuleType(IRuleType ruleType);

        /// <summary>
        /// Generuje nový unikátní ID pro pravidlo.
        /// </summary>
        /// <returns>Nové ID pravidla.</returns>
        int GetNextRuleId();

        #region SingleResponsibilityRule

        /// <summary>
        /// Vytvoří jednoduché pravidlo s jednou odpovědností.
        /// </summary>
        /// <typeparam name="TInput">Typ validovaných dat.</typeparam>
        /// <param name="validateFunc">Validační funkce.</param>
        /// <param name="ruleType">Typ pravidla.</param>
        /// <param name="ruleId">Volitelný jedinečný identifikátor pravidla.</param>
        /// <param name="name">Název pravidla.</param>
        /// <param name="description">Popis pravidla.</param>
        /// <param name="priority">Priorita pravidla.</param>
        /// <param name="isActive">Zda je pravidlo aktivní.</param>
        /// <param name="timestamp">Časová značka pravidla.</param>
        /// <returns>Vytvořené pravidlo.</returns>
        SingleResponsibilityRule<TInput> CreateSimpleRule<TInput>(
            SingleResponsibilityRule<TInput>.ValidateDelegate validateFunc,
            IRuleType ruleType,
            string? ruleId = null,
            string? name = null,
            string? description = null,
            int priority = 0,
            bool isActive = true,
            DateTimeOffset? timestamp = null);

        #endregion

        #region ConditionalRule

        /// <summary>
        /// Vytvoří podmínkové pravidlo (if-then-else).
        /// </summary>
        /// <typeparam name="TInput">Typ validovaných dat.</typeparam>
        /// <param name="conditionFunc">Funkce podmínky.</param>
        /// <param name="thenRule">Pravidlo pro THEN větev.</param>
        /// <param name="elseRule">Pravidlo pro ELSE větev.</param>
        /// <param name="ruleType">Typ pravidla.</param>
        /// <param name="ruleId">Volitelný jedinečný identifikátor pravidla.</param>
        /// <param name="name">Název pravidla.</param>
        /// <param name="description">Popis pravidla.</param>
        /// <param name="priority">Priorita pravidla.</param>
        /// <param name="isActive">Zda je pravidlo aktivní.</param>
        /// <param name="timestamp">Časová značka pravidla.</param>
        /// <returns>Vytvořené podmínkové pravidlo.</returns>
        ConditionalRule<TInput> CreateConditionalRule<TInput>(
            ConditionalRule<TInput>.ConditionDelegate conditionFunc,
            Rule<TInput>? thenRule,
            Rule<TInput>? elseRule,
            IRuleType ruleType,
            string? ruleId = null,
            string? name = null,
            string? description = null,
            int priority = 0,
            bool isActive = true,
            DateTimeOffset? timestamp = null);

        #endregion

        #region CompositeRule

        /// <summary>
        /// Vytvoří kompozitní pravidlo typu AND (všechna pravidla musí být splněna).
        /// </summary>
        /// <typeparam name="TInput">Typ validovaných dat.</typeparam>
        /// <param name="rules">Seznam pravidel v kompozitu.</param>
        /// <param name="ruleType">Typ pravidla.</param>
        /// <param name="ruleId">Volitelný jedinečný identifikátor pravidla.</param>
        /// <param name="name">Název pravidla.</param>
        /// <param name="description">Popis pravidla.</param>
        /// <param name="priority">Priorita pravidla.</param>
        /// <param name="isActive">Zda je pravidlo aktivní.</param>
        /// <param name="timestamp">Časová značka pravidla.</param>
        /// <returns>Vytvořené kompozitní pravidlo typu AND.</returns>
        CompositeRule<TInput> CreateAndCompositeRule<TInput>(
            IEnumerable<Rule<TInput>> rules,
            IRuleType ruleType,
            string? ruleId = null,
            string? name = null,
            string? description = null,
            int priority = 0,
            bool isActive = true,
            DateTimeOffset? timestamp = null);

        /// <summary>
        /// Vytvoří kompozitní pravidlo typu OR (alespoň jedno pravidlo musí být splněno).
        /// </summary>
        /// <typeparam name="TInput">Typ validovaných dat.</typeparam>
        /// <param name="rules">Seznam pravidel v kompozitu.</param>
        /// <param name="ruleType">Typ pravidla.</param>
        /// <param name="ruleId">Volitelný jedinečný identifikátor pravidla.</param>
        /// <param name="name">Název pravidla.</param>
        /// <param name="description">Popis pravidla.</param>
        /// <param name="priority">Priorita pravidla.</param>
        /// <param name="isActive">Zda je pravidlo aktivní.</param>
        /// <param name="timestamp">Časová značka pravidla.</param>
        /// <returns>Vytvořené kompozitní pravidlo typu OR.</returns>
        CompositeRule<TInput> CreateOrCompositeRule<TInput>(
            IEnumerable<Rule<TInput>> rules,
            IRuleType ruleType,
            string? ruleId = null,
            string? name = null,
            string? description = null,
            int priority = 0,
            bool isActive = true,
            DateTimeOffset? timestamp = null);

        /// <summary>
        /// Vytvoří kompozitní pravidlo s minimálním počtem splněných pravidel.
        /// </summary>
        /// <typeparam name="TInput">Typ validovaných dat.</typeparam>
        /// <param name="minimumCount">Minimální počet splněných pravidel.</param>
        /// <param name="rules">Seznam pravidel v kompozitu.</param>
        /// <param name="ruleType">Typ pravidla.</param>
        /// <param name="ruleId">Volitelný jedinečný identifikátor pravidla.</param>
        /// <param name="name">Název pravidla.</param>
        /// <param name="description">Popis pravidla.</param>
        /// <param name="priority">Priorita pravidla.</param>
        /// <param name="isActive">Zda je pravidlo aktivní.</param>
        /// <param name="timestamp">Časová značka pravidla.</param>
        /// <returns>Vytvořené kompozitní pravidlo s minimálním počtem.</returns>
        CompositeRule<TInput> CreateMinimumCompositeRule<TInput>(
            int minimumCount,
            IEnumerable<Rule<TInput>> rules,
            IRuleType ruleType,
            string? ruleId = null,
            string? name = null,
            string? description = null,
            int priority = 0,
            bool isActive = true,
            DateTimeOffset? timestamp = null);

        /// <summary>
        /// Vytvoří kompozitní pravidlo s minimálním procentem splněných pravidel.
        /// </summary>
        /// <typeparam name="TInput">Typ validovaných dat.</typeparam>
        /// <param name="minimumPercentage">Minimální procento splněných pravidel (0-100).</param>
        /// <param name="rules">Seznam pravidel v kompozitu.</param>
        /// <param name="ruleType">Typ pravidla.</param>
        /// <param name="ruleId">Volitelný jedinečný identifikátor pravidla.</param>
        /// <param name="name">Název pravidla.</param>
        /// <param name="description">Popis pravidla.</param>
        /// <param name="priority">Priorita pravidla.</param>
        /// <param name="isActive">Zda je pravidlo aktivní.</param>
        /// <param name="timestamp">Časová značka pravidla.</param>
        /// <returns>Vytvořené kompozitní pravidlo s minimálním procentem.</returns>
        CompositeRule<TInput> CreatePercentageCompositeRule<TInput>(
            double minimumPercentage,
            IEnumerable<Rule<TInput>> rules,
            IRuleType ruleType,
            string? ruleId = null,
            string? name = null,
            string? description = null,
            int priority = 0,
            bool isActive = true,
            DateTimeOffset? timestamp = null);

        /// <summary>
        /// Vytvoří kompozitní pravidlo s vlastní funkcí vyhodnocení.
        /// </summary>
        /// <typeparam name="TInput">Typ validovaných dat.</typeparam>
        /// <param name="evaluateFunc">Vlastní funkce vyhodnocení.</param>
        /// <param name="rules">Seznam pravidel v kompozitu.</param>
        /// <param name="ruleType">Typ pravidla.</param>
        /// <param name="ruleId">Volitelný jedinečný identifikátor pravidla.</param>
        /// <param name="name">Název pravidla.</param>
        /// <param name="description">Popis pravidla.</param>
        /// <param name="priority">Priorita pravidla.</param>
        /// <param name="isActive">Zda je pravidlo aktivní.</param>
        /// <param name="timestamp">Časová značka pravidla.</param>
        /// <returns>Vytvořené kompozitní pravidlo s vlastní funkcí vyhodnocení.</returns>
        CompositeRule<TInput> CreateCustomCompositeRule<TInput>(
            CompositeRule<TInput>.EvaluateChildrenDelegate evaluateFunc,
            IEnumerable<Rule<TInput>> rules,
            IRuleType ruleType,
            string? ruleId = null,
            string? name = null,
            string? description = null,
            int priority = 0,
            bool isActive = true,
            DateTimeOffset? timestamp = null);

        #endregion

        #region DependentRule

        /// <summary>
        /// Vytvoří závislé pravidlo.
        /// </summary>
        /// <typeparam name="TInput">Typ validovaných dat.</typeparam>
        /// <param name="evaluateFunc">Funkce pro vyhodnocení výsledků závislých pravidel.</param>
        /// <param name="dependencies">Seznam závislých pravidel.</param>
        /// <param name="ruleType">Typ pravidla.</param>
        /// <param name="ruleId">Volitelný jedinečný identifikátor pravidla.</param>
        /// <param name="name">Název pravidla.</param>
        /// <param name="description">Popis pravidla.</param>
        /// <param name="priority">Priorita pravidla.</param>
        /// <param name="isActive">Zda je pravidlo aktivní.</param>
        /// <param name="timestamp">Časová značka pravidla.</param>
        /// <returns>Vytvořené závislé pravidlo.</returns>
        DependentRule<TInput> CreateDependentRule<TInput>(
            DependentRule<TInput>.EvaluateDependenciesDelegate evaluateFunc,
            IEnumerable<Rule<TInput>> dependencies,
            IRuleType ruleType,
            string? ruleId = null,
            string? name = null,
            string? description = null,
            int priority = 0,
            bool isActive = true,
            DateTimeOffset? timestamp = null);

        #endregion

        #region SwitchRule

        /// <summary>
        /// Vytvoří přepínací pravidlo.
        /// </summary>
        /// <typeparam name="TInput">Typ validovaných dat.</typeparam>
        /// <param name="switchKeyFunc">Funkce pro získání přepínacího klíče.</param>
        /// <param name="defaultCase">Výchozí pravidlo.</param>
        /// <param name="cases">Slovník případů.</param>
        /// <param name="ruleType">Typ pravidla.</param>
        /// <param name="ruleId">Volitelný jedinečný identifikátor pravidla.</param>
        /// <param name="name">Název pravidla.</param>
        /// <param name="description">Popis pravidla.</param>
        /// <param name="priority">Priorita pravidla.</param>
        /// <param name="isActive">Zda je pravidlo aktivní.</param>
        /// <param name="timestamp">Časová značka pravidla.</param>
        /// <returns>Vytvořené přepínací pravidlo.</returns>
        SwitchRule<TInput> CreateSwitchRule<TInput>(
            SwitchRule<TInput>.SwitchKeyDelegate switchKeyFunc,
            Rule<TInput>? defaultCase,
            IDictionary<object, Rule<TInput>> cases,
            IRuleType ruleType,
            string? ruleId = null,
            string? name = null,
            string? description = null,
            int priority = 0,
            bool isActive = true,
            DateTimeOffset? timestamp = null);

        #endregion

        #region PredictiveRule

        /// <summary>
        /// Vytvoří prediktivní pravidlo.
        /// </summary>
        /// <typeparam name="TInput">Typ validovaných dat.</typeparam>
        /// <typeparam name="THistoryData">Typ historických dat používaných pro predikci.</typeparam>
        /// <param name="predictFunc">Prediktivní funkce.</param>
        /// <param name="historicalData">Historická data pro predikci.</param>
        /// <param name="ruleType">Typ pravidla.</param>
        /// <param name="ruleId">Volitelný jedinečný identifikátor pravidla.</param>
        /// <param name="name">Název pravidla.</param>
        /// <param name="description">Popis pravidla.</param>
        /// <param name="priority">Priorita pravidla.</param>
        /// <param name="isActive">Zda je pravidlo aktivní.</param>
        /// <param name="timestamp">Časová značka pravidla.</param>
        /// <returns>Vytvořené prediktivní pravidlo.</returns>
        PredictiveRule<TInput, THistoryData> CreatePredictiveRule<TInput, THistoryData>(
            PredictiveRule<TInput, THistoryData>.PredictDelegate predictFunc,
            IEnumerable<THistoryData>? historicalData,
            IRuleType ruleType,
            string? ruleId = null,
            string? name = null,
            string? description = null,
            int priority = 0,
            bool isActive = true,
            DateTimeOffset? timestamp = null);

        #endregion
    }
}
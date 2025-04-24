// Engine/Validation/Core/Validators/Dependency/RuleDependencyEvaluator.cs
using Ruleflow.NET.Engine.Validation.Core.Context;
using Ruleflow.NET.Engine.Validation.Enums;
using Ruleflow.NET.Engine.Validation.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ruleflow.NET.Engine.Validation.Core.Validators.Dependency
{
    /// <summary>
    /// Vyhodnocuje podmínky pro spuštění závislých pravidel na základě výsledků jiných pravidel.
    /// </summary>
    internal class RuleDependencyEvaluator
    {
        /// <summary>
        /// Vyhodnotí, zda má být pravidlo spuštěno na základě stavu jeho závislostí.
        /// </summary>
        /// <typeparam name="T">Typ validovaných dat</typeparam>
        /// <param name="rule">Závislé pravidlo k vyhodnocení</param>
        /// <param name="context">Validační kontext obsahující výsledky předchozích pravidel</param>
        /// <returns>True, pokud by pravidlo mělo být spuštěno; jinak false</returns>
        public bool ShouldProcessRule<T>(IDependentValidationRule<T> rule, ValidationContext context)
        {
            var dependsOn = rule.DependsOn;
            if (!dependsOn.Any()) return true;

            return rule.DependencyType switch
            {
                DependencyType.RequiresAllSuccess => AllRulesSucceeded(context, dependsOn),
                DependencyType.RequiresAnySuccess => AnyRuleSucceeded(context, dependsOn),
                DependencyType.RequiresAllFailure => AllRulesFailed(context, dependsOn),
                DependencyType.RequiresAnyFailure => AnyRuleFailed(context, dependsOn),
                _ => throw new ArgumentException($"Nepodporovaný typ závislosti: {rule.DependencyType}")
            };
        }

        /// <summary>
        /// Zkontroluje, zda všechna pravidla s danými ID byla úspěšně vyhodnocena.
        /// </summary>
        /// <param name="context">Validační kontext</param>
        /// <param name="ruleIds">ID pravidel ke kontrole</param>
        /// <returns>True, pokud všechna pravidla byla úspěšně vyhodnocena; jinak false</returns>
        private bool AllRulesSucceeded(ValidationContext context, IEnumerable<string> ruleIds)
        {
            return ruleIds.All(ruleId => HasRuleSucceeded(context, ruleId));
        }

        /// <summary>
        /// Zkontroluje, zda alespoň jedno pravidlo s daným ID bylo úspěšně vyhodnoceno.
        /// </summary>
        /// <param name="context">Validační kontext</param>
        /// <param name="ruleIds">ID pravidel ke kontrole</param>
        /// <returns>True, pokud alespoň jedno pravidlo bylo úspěšně vyhodnoceno; jinak false</returns>
        private bool AnyRuleSucceeded(ValidationContext context, IEnumerable<string> ruleIds)
        {
            return ruleIds.Any(ruleId => HasRuleSucceeded(context, ruleId));
        }

        /// <summary>
        /// Zkontroluje, zda všechna pravidla s danými ID selhala.
        /// </summary>
        /// <param name="context">Validační kontext</param>
        /// <param name="ruleIds">ID pravidel ke kontrole</param>
        /// <returns>True, pokud všechna pravidla selhala; jinak false</returns>
        private bool AllRulesFailed(ValidationContext context, IEnumerable<string> ruleIds)
        {
            return ruleIds.All(ruleId => HasRuleFailed(context, ruleId));
        }

        /// <summary>
        /// Zkontroluje, zda alespoň jedno pravidlo s daným ID selhalo.
        /// </summary>
        /// <param name="context">Validační kontext</param>
        /// <param name="ruleIds">ID pravidel ke kontrole</param>
        /// <returns>True, pokud alespoň jedno pravidlo selhalo; jinak false</returns>
        private bool AnyRuleFailed(ValidationContext context, IEnumerable<string> ruleIds)
        {
            return ruleIds.Any(ruleId => HasRuleFailed(context, ruleId));
        }

        /// <summary>
        /// Zkontroluje, zda pravidlo s daným ID bylo úspěšně vyhodnoceno.
        /// </summary>
        /// <param name="context">Validační kontext</param>
        /// <param name="ruleId">ID pravidla ke kontrole</param>
        /// <returns>True, pokud pravidlo bylo úspěšně vyhodnoceno; jinak false</returns>
        private bool HasRuleSucceeded(ValidationContext context, string ruleId)
        {
            return context.RuleResults.TryGetValue(ruleId, out var result) && result.Success;
        }

        /// <summary>
        /// Zkontroluje, zda pravidlo s daným ID selhalo.
        /// </summary>
        /// <param name="context">Validační kontext</param>
        /// <param name="ruleId">ID pravidla ke kontrole</param>
        /// <returns>True, pokud pravidlo selhalo; jinak false</returns>
        private bool HasRuleFailed(ValidationContext context, string ruleId)
        {
            return context.RuleResults.TryGetValue(ruleId, out var result) && !result.Success;
        }
    }
}
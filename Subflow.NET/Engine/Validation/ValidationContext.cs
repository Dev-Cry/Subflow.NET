using Ruleflow.NET.Engine.Validation.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Ruleflow.NET.Engine.Validation
{
    /// <summary>
    /// Poskytuje kontext pro validační proces, včetně výsledků jednotlivých pravidel
    /// a uživatelských vlastností.
    /// </summary>
    public class ValidationContext
    {
        private readonly Dictionary<string, ValidationRuleResult> _ruleResults = new();

        /// <summary>
        /// Výsledky jednotlivých validačních pravidel
        /// </summary>
        public IReadOnlyDictionary<string, ValidationRuleResult> RuleResults => _ruleResults;

        /// <summary>
        /// Uživatelská data pro přenos mezi pravidly
        /// </summary>
        public Dictionary<string, object> Properties { get; } = new();

        /// <summary>
        /// Cancellation token pro možnost zrušení validace
        /// </summary>
        public CancellationToken CancellationToken { get; set; } = CancellationToken.None;

        /// <summary>
        /// Přidá výsledek validace pravidla do kontextu
        /// </summary>
        public void AddRuleResult(ValidationRuleResult result)
        {
            _ruleResults[result.RuleId] = result;
        }

        /// <summary>
        /// Zjistí, zda pravidlo s daným ID bylo úspěšně vyhodnoceno
        /// </summary>
        public bool HasRuleSucceeded(string ruleId)
        {
            return _ruleResults.TryGetValue(ruleId, out var result) && result.Success;
        }

        /// <summary>
        /// Zjistí, zda pravidlo s daným ID selhalo
        /// </summary>
        public bool HasRuleFailed(string ruleId)
        {
            return _ruleResults.TryGetValue(ruleId, out var result) && !result.Success;
        }

        /// <summary>
        /// Zjistí, zda všechna pravidla s danými ID byla úspěšně vyhodnocena
        /// </summary>
        public bool AllRulesSucceeded(IEnumerable<string> ruleIds)
        {
            return ruleIds.All(HasRuleSucceeded);
        }

        /// <summary>
        /// Zjistí, zda alespoň jedno pravidlo s daným ID bylo úspěšně vyhodnoceno
        /// </summary>
        public bool AnyRuleSucceeded(IEnumerable<string> ruleIds)
        {
            return ruleIds.Any(HasRuleSucceeded);
        }

        /// <summary>
        /// Zjistí, zda všechna pravidla s danými ID selhala
        /// </summary>
        public bool AllRulesFailed(IEnumerable<string> ruleIds)
        {
            return ruleIds.All(HasRuleFailed);
        }

        /// <summary>
        /// Zjistí, zda alespoň jedno pravidlo s daným ID selhalo
        /// </summary>
        public bool AnyRuleFailed(IEnumerable<string> ruleIds)
        {
            return ruleIds.Any(HasRuleFailed);
        }
    }
}
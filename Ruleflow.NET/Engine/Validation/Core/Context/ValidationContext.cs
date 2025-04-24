// Detailní oprava pro ValidationContext.cs
using Ruleflow.NET.Engine.Validation.Core.Results;
using Ruleflow.NET.Engine.Validation.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Ruleflow.NET.Engine.Validation.Core.Context
{
    /// <summary>
    /// Poskytuje kontext pro validační proces, včetně výsledků jednotlivých pravidel
    /// a uživatelských vlastností.
    /// </summary>
    public class ValidationContext
    {
        // Singleton instance
        private static readonly object _lockObj = new object();
        private static ValidationContext _instance;

        // Dictionary pro ukládání výsledků pravidel podle ID
        private readonly Dictionary<string, ValidationRuleResult> _ruleResults = new Dictionary<string, ValidationRuleResult>();

        // Dictionary pro ukládání uživatelských vlastností
        private readonly Dictionary<string, object> _properties = new Dictionary<string, object>();

        /// <summary>
        /// Privátní konstruktor pro singleton
        /// </summary>
        public ValidationContext()
        {
        }

        /// <summary>
        /// Získá sdílenou instanci kontextu validace.
        /// </summary>
        public static ValidationContext Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lockObj)
                    {
                        if (_instance == null)
                        {
                            _instance = new ValidationContext();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Výsledky jednotlivých validačních pravidel
        /// </summary>
        public IReadOnlyDictionary<string, ValidationRuleResult> RuleResults
        {
            get
            {
                lock (_lockObj)
                {
                    return new Dictionary<string, ValidationRuleResult>(_ruleResults);
                }
            }
        }

        /// <summary>
        /// Uživatelská data pro přenos mezi pravidly
        /// </summary>
        public Dictionary<string, object> Properties
        {
            get
            {
                return _properties;
            }
        }

        /// <summary>
        /// Cancellation token pro možnost zrušení validace
        /// </summary>
        public CancellationToken CancellationToken { get; set; } = CancellationToken.None;

        /// <summary>
        /// Přidá výsledek validace pravidla do kontextu
        /// </summary>
        public void AddRuleResult(ValidationRuleResult result)
        {
            if (result == null)
                return;

            lock (_lockObj)
            {
                _ruleResults[result.RuleId] = result;
            }
        }

        /// <summary>
        /// Zjistí, zda pravidlo s daným ID bylo úspěšně vyhodnoceno
        /// </summary>
        public bool HasRuleSucceeded(string ruleId)
        {
            lock (_lockObj)
            {
                return _ruleResults.TryGetValue(ruleId, out var result) && result.Success;
            }
        }

        /// <summary>
        /// Zjistí, zda pravidlo s daným ID selhalo
        /// </summary>
        public bool HasRuleFailed(string ruleId)
        {
            lock (_lockObj)
            {
                return _ruleResults.TryGetValue(ruleId, out var result) && !result.Success;
            }
        }

        /// <summary>
        /// Zjistí, zda všechna pravidla s danými ID byla úspěšně vyhodnocena
        /// </summary>
        public bool AllRulesSucceeded(IEnumerable<string> ruleIds)
        {
            if (ruleIds == null || !ruleIds.Any())
                return true;

            lock (_lockObj)
            {
                return ruleIds.All(HasRuleSucceeded);
            }
        }

        /// <summary>
        /// Zjistí, zda alespoň jedno pravidlo s daným ID bylo úspěšně vyhodnoceno
        /// </summary>
        public bool AnyRuleSucceeded(IEnumerable<string> ruleIds)
        {
            if (ruleIds == null || !ruleIds.Any())
                return false;

            lock (_lockObj)
            {
                return ruleIds.Any(HasRuleSucceeded);
            }
        }

        /// <summary>
        /// Zjistí, zda všechna pravidla s danými ID selhala
        /// </summary>
        public bool AllRulesFailed(IEnumerable<string> ruleIds)
        {
            if (ruleIds == null || !ruleIds.Any())
                return true;

            lock (_lockObj)
            {
                return ruleIds.All(HasRuleFailed);
            }
        }

        /// <summary>
        /// Zjistí, zda alespoň jedno pravidlo s daným ID selhalo
        /// </summary>
        public bool AnyRuleFailed(IEnumerable<string> ruleIds)
        {
            if (ruleIds == null || !ruleIds.Any())
                return false;

            lock (_lockObj)
            {
                return ruleIds.Any(HasRuleFailed);
            }
        }

        /// <summary>
        /// Vyčistí všechny výsledky pravidel a vlastnosti
        /// </summary>
        public void Clear()
        {
            lock (_lockObj)
            {
                _ruleResults.Clear();
                _properties.Clear();
            }
        }

        /// <summary>
        /// Vrátí hodnotu Dictionary-safe, pokud klíč neexistuje, vrátí výchozí hodnotu
        /// </summary>
        public T GetPropertyOrDefault<T>(string key, T defaultValue = default)
        {
            lock (_lockObj)
            {
                if (_properties.TryGetValue(key, out var value) && value is T typedValue)
                {
                    return typedValue;
                }
                return defaultValue;
            }
        }

        /// <summary>
        /// Přidá nebo aktualizuje vlastnost v kontextu
        /// </summary>
        public void SetProperty(string key, object value)
        {
            lock (_lockObj)
            {
                _properties[key] = value;
            }
        }

        /// <summary>
        /// Vrátí všechny výsledky pravidel s danou úrovní úspěchu
        /// </summary>
        public IEnumerable<ValidationRuleResult> GetRuleResultsBySuccess(bool success)
        {
            lock (_lockObj)
            {
                return _ruleResults.Values.Where(r => r.Success == success).ToList();
            }
        }

        /// <summary>
        /// Výpis stavu kontextu pro ladění
        /// </summary>
        public override string ToString()
        {
            lock (_lockObj)
            {
                var ruleResultsStr = string.Join(", ", _ruleResults.Select(r => $"{r.Key}={r.Value.Success}"));
                var propertiesStr = string.Join(", ", _properties.Select(p => $"{p.Key}={p.Value}"));
                return $"ValidationContext: Rules=[{ruleResultsStr}], Properties=[{propertiesStr}]";
            }
        }
    }
}
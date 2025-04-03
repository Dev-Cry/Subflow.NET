namespace Ruleflow.NET.Engine.Validation
{
    /// <summary>
    /// Reprezentuje výsledek vyhodnocení jednoho validačního pravidla
    /// </summary>
    public class ValidationRuleResult
    {
        /// <summary>
        /// Identifikátor validačního pravidla
        /// </summary>
        public string RuleId { get; }

        /// <summary>
        /// Určuje, zda validace byla úspěšná
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Chyba validace, pokud validace selhala
        /// </summary>
        public ValidationError? Error { get; }

        /// <summary>
        /// Vytvoří novou instanci výsledku validace
        /// </summary>
        public ValidationRuleResult(string ruleId, bool success, ValidationError? error = null)
        {
            RuleId = ruleId;
            Success = success;
            Error = error;
        }
    }
}
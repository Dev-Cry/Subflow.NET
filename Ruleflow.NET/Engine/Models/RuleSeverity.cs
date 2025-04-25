namespace Ruleflow.NET.Engine.Models
{
    /// <summary>
    /// Defines severity levels for validation rules and results.
    /// </summary>
    public enum RuleSeverity
    {
        /// <summary>
        /// Informational message that doesn't affect validation success.
        /// </summary>
        Information = 0,

        /// <summary>
        /// Suggestion that doesn't affect validation success.
        /// </summary>
        Suggestion = 25,

        /// <summary>
        /// Warning that doesn't cause validation to fail but should be addressed.
        /// </summary>
        Warning = 50,

        /// <summary>
        /// Error condition that causes validation to fail.
        /// </summary>
        Error = 75,

        /// <summary>
        /// Critical error condition that causes validation to fail immediately.
        /// </summary>
        Critical = 100
    }
}
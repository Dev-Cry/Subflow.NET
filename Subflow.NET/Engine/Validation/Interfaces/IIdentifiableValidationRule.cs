namespace Subflow.NET.Engine.Validation.Interfaces
{
    /// <summary>
    /// Rozhraní pro validační pravidla s jednoznačným identifikátorem
    /// </summary>
    public interface IIdentifiableValidationRule<T> : IValidationRule<T>
    {
        /// <summary>
        /// Jedinečný identifikátor pravidla
        /// </summary>
        string RuleId { get; }
    }
}
namespace Subflow.NET.Engine.Validation.Interfaces
{
    /// <summary>
    /// Rozhraní pro validační pravidla s definovanou prioritou
    /// </summary>
    public interface IPrioritizedValidationRule<T> : IValidationRule<T>
    {
        /// <summary>
        /// Priorita pravidla určuje pořadí vyhodnocování (vyšší priorita = dřívější vyhodnocení)
        /// </summary>
        int Priority { get; }
    }
}
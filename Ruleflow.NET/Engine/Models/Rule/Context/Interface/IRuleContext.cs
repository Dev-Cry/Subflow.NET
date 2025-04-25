namespace Ruleflow.NET.Engine.Models.Rule.Context.Interface
{
    /// <summary>
    /// Rozhraní pro kontext pravidla.
    /// </summary>
    public interface IRuleContext
    {
        /// <summary>
        /// Unikátní identifikátor kontextu.
        /// </summary>
        Guid ContextId { get; }

        /// <summary>
        /// Název kontextu.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Volitelný popis kontextu.
        /// </summary>
        string? Description { get; }

        /// <summary>
        /// Parametry kontextu.
        /// </summary>
        IReadOnlyDictionary<string, object> Parameters { get; }

        /// <summary>
        /// Časová značka vytvoření kontextu.
        /// </summary>
        DateTimeOffset Timestamp { get; }

        /// <summary>
        /// Přidá nebo aktualizuje parametr v kontextu.
        /// </summary>
        void AddParameter(string key, object value);
    }
}
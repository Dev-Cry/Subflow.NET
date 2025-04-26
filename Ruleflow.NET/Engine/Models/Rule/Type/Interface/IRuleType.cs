namespace Ruleflow.NET.Engine.Models.Rule.Type.Interface
{
    /// <summary>
    /// Rozhraní pro typ pravidla.
    /// </summary>
    public interface IRuleType<TInput>
    {
        /// <summary>
        /// Unikátní identifikátor typu pravidla.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Kód typu pravidla (zkratka).
        /// </summary>
        string Code { get; }

        /// <summary>
        /// Název typu pravidla.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Volitelný popis typu pravidla.
        /// </summary>
        string? Description { get; }

        /// <summary>
        /// Indikuje, zda je typ pravidla aktivní.
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Datum a čas vytvoření typu pravidla.
        /// </summary>
        DateTimeOffset CreatedAt { get; }
    }
}
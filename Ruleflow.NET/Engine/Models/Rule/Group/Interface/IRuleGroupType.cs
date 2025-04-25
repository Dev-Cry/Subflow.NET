namespace Ruleflow.NET.Engine.Models.Rule.Group.Interface
{
    /// <summary>
    /// Rozhraní pro typ skupiny pravidel.
    /// </summary>
    public interface IRuleGroupType
    {
        /// <summary>
        /// Unikátní identifikátor typu skupiny.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Kód typu skupiny (zkratka).
        /// </summary>
        string Code { get; }

        /// <summary>
        /// Název typu skupiny.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Volitelný popis typu skupiny.
        /// </summary>
        string? Description { get; }

        /// <summary>
        /// Indikuje, zda je typ skupiny aktivní.
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Datum a čas vytvoření typu skupiny.
        /// </summary>
        DateTimeOffset CreatedAt { get; }
    }
}
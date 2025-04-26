using System;

namespace Ruleflow.NET.Engine.Models.Rule.Group.Interface
{
    /// <summary>
    /// Rozhraní pro typ skupiny pravidel s typovou vazbou na vstupní data.
    /// </summary>
    /// <typeparam name="TInput">Typ validovaných dat.</typeparam>
    public interface IRuleGroupType<TInput>
    {
        /// <summary>
        /// Unikátní identifikátor typu skupiny.
        /// </summary>
        new int Id { get; }

        /// <summary>
        /// Kód typu skupiny (zkratka).
        /// </summary>
        new string Code { get; }

        /// <summary>
        /// Název typu skupiny.
        /// </summary>
        new string Name { get; }

        /// <summary>
        /// Volitelný popis typu skupiny.
        /// </summary>
        new string? Description { get; }

        /// <summary>
        /// Indikuje, zda je typ skupiny aktivní.
        /// </summary>
        new bool IsEnabled { get; }

        /// <summary>
        /// Datum a čas vytvoření typu skupiny.
        /// </summary>
        new DateTimeOffset CreatedAt { get; }
    }
}
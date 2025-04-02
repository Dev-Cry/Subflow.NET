using Subflow.NET.Engine.Validation.Enums;
using System.Collections.Generic;

namespace Subflow.NET.Engine.Validation.Interfaces
{
    /// <summary>
    /// Rozhraní pro validační pravidla, která závisí na výsledcích jiných pravidel
    /// </summary>
    public interface IDependentValidationRule<T> : IValidationRule<T>
    {
        /// <summary>
        /// Identifikátory pravidel, na kterých toto pravidlo závisí
        /// </summary>
        IEnumerable<string> DependsOn { get; }

        /// <summary>
        /// Typ závislosti, který určuje, kdy se pravidlo má spustit
        /// </summary>
        DependencyType DependencyType { get; }
    }
}
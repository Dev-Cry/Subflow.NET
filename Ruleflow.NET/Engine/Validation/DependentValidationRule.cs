using Ruleflow.NET.Engine.Validation.Enums;
using Ruleflow.NET.Engine.Validation.Interfaces;
using System.Collections.Generic;

namespace Ruleflow.NET.Engine.Validation
{
    /// <summary>
    /// Základní třída pro validační pravidla závislá na výsledcích jiných pravidel
    /// </summary>
    public abstract class DependentValidationRule<T> : IdentifiableValidationRule<T>, IDependentValidationRule<T>
    {
        private readonly IEnumerable<string> _dependsOn;
        private readonly DependencyType _dependencyType;

        protected DependentValidationRule(string ruleId, IEnumerable<string> dependsOn, DependencyType dependencyType)
            : base(ruleId)
        {
            _dependsOn = dependsOn;
            _dependencyType = dependencyType;
        }

        /// <summary>
        /// Identifikátory pravidel, na kterých toto pravidlo závisí
        /// </summary>
        public IEnumerable<string> DependsOn => _dependsOn;

        /// <summary>
        /// Typ závislosti, který určuje, kdy se pravidlo má spustit
        /// </summary>
        public DependencyType DependencyType => _dependencyType;
    }
}
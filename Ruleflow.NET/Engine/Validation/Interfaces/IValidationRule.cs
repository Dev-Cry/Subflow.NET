using Ruleflow.NET.Engine.Validation.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Ruleflow.NET.Engine.Validation.Interfaces
{
    public interface IValidationRule<T>
    {
        void Validate(T input);

        ValidationSeverity DefaultSeverity { get; }
    }

    public interface IValidationRule
    {
        Type InputType { get; }
    }
}

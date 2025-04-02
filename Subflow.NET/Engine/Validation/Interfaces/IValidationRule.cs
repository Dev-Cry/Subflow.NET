using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// --- IValidationRule<T>.cs ---
namespace Subflow.NET.Engine.Validation.Interfaces
{
    public interface IValidationRule<T>
    {
        void Validate(T input);
    }

    public interface IValidationRule
    {
        Type InputType { get; }
    }
}

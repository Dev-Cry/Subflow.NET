using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subflow.NET.IO.Loader.Validation
{
    public interface IValidator<T>
    {
        void Validate(T input);
    }
}

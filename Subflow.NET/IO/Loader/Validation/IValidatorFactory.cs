using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subflow.NET.IO.Loader.Validation
{
    // Factory pro vytvoření validátorů
    public interface IValidatorFactory
    {
        IValidator<string> CreatePathValidator();
        IValidator<FileInfo> CreateFileValidator();
    }
}

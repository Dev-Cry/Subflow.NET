using Subflow.NET.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subflow.NET.Parser
{
    public interface ISubtitleParser
    {
        Task<ISubtitle> ParseLineAsync(string line);

        Task<ISubtitle> FlushAsync();
    }
}

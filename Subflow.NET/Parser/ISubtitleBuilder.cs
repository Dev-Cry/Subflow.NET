using Subflow.NET.Data.Model;
using System.Threading.Tasks;

namespace Subflow.NET.Parser
{
    public interface ISubtitleBuilder
    {
        Task<ISubtitle?> ParseLineAsync(string line, ISubtitleTimeParser timeParser);
        Task<ISubtitle?> FlushAsync();
    }
}
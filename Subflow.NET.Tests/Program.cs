using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Subflow.NET.IO.Loader;
using Subflow.NET.IO.Reader;
using Subflow.NET.Parser;
using Subflow.NET.Data.Model;

namespace Subflow.NET.Tests
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection()
                .AddLogging(builder => builder.AddConsole())
                .AddSingleton<ISubtitleParser, SubtitleParser>()
                .AddSingleton<IFileReader>(provider => new FileReader("C:\\Users\\tomas\\source\\repos\\Subflow.NET\\Subflow.NET.Tests\\cze.srt", Encoding.UTF8))
                .AddSingleton<IFileLoader, FileLoader>()
                .BuildServiceProvider();

            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<Program>();

            var fileLoader = services.GetRequiredService<IFileLoader>();

            // Načtení titulků přes FileLoader
            await foreach (var subtitle in fileLoader.LoadFileAsync("C:\\Users\\tomas\\source\\repos\\Subflow.NET\\Subflow.NET.Tests\\cze.srt"))
            {
                Console.WriteLine(subtitle);
            }

            logger.LogInformation("Načtení titulků přes rozhraní dokončeno.");
        }
    }
}

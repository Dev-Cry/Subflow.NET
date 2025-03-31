using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Subflow.NET.IO.Loader;
using Subflow.NET.IO.Reader;
using Subflow.NET.Parser;
using Subflow.NET.IO.Loader.Validation;
using Subflow.NET.IO.Loader.Validation.Rules;
using Subflow.NET.Data.Model;

class Program
{
    static async Task Main(string[] args)
    {
        var filePath = "C:\\Users\\tomas\\source\\repos\\Subflow.NET\\Subflow.NET.Tests\\cze.srt"; // uprav dle potřeby

        var services = new ServiceCollection();

        // Logging
        services.AddLogging(builder =>
        {
            builder.AddSimpleConsole(options =>
            {
                options.SingleLine = true;
                options.TimestampFormat = "[HH:mm:ss] ";
            });
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Subflow.NET závislosti
        services.AddSingleton<ISubtitleLineReader>(sp => new SubtitleLineReader(filePath, Encoding.UTF8));
        services.AddSingleton<ISubtitleParser, SubtitleParser>();
        services.AddSingleton<ISubtitleTimeParser, SubtitleTimeParser>();
        services.AddSingleton<ISubtitleBuilder, SubtitleBuilder>();
        services.AddSingleton<IValidatorFactory, ValidatorFactory>();
        services.AddSingleton<IBufferSizeDeterminer, BufferSizeDeterminer>();
        services.AddSingleton<IFileLoader, FileLoader>();

        var provider = services.BuildServiceProvider();

        var fileLoader = provider.GetRequiredService<IFileLoader>();
        var subtitles = new List<ISubtitle>();

        try
        {
            await foreach (var subtitle in fileLoader.LoadFileAsync(filePath, degreeOfParallelism: 1))
            {
                subtitles.Add(subtitle);
            }

            Console.WriteLine("\n------ VÝPIS VŠECH TITULKŮ ------");
            foreach (var subtitle in subtitles)
            {
                Console.WriteLine(subtitle); // ToString() ve tvém ISubtitle by měl zobrazit formátovaný výstup
                Console.WriteLine();
            }
        }
        catch (AggregateException ex)
        {
            Console.WriteLine("Došlo k validační chybě:");
            foreach (var error in ex.InnerExceptions)
            {
                Console.WriteLine($"- {error.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Neočekávaná chyba: {ex.Message}");
        }
    }
}

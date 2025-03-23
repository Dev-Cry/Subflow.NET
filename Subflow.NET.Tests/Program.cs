using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Subflow.NET.IO.Loader;
using Subflow.NET.IO.Reader;
using Subflow.NET.Parser;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Subflow.NET.Data.Model;

class Program
{
    static async Task Main(string[] args)
    {
        var filePath = "C:\\Users\\tomas\\source\\repos\\Subflow.NET\\Subflow.NET.Tests\\cze.srt"; // změň dle potřeby

        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddSimpleConsole(o =>
            {
                o.SingleLine = true;
                o.TimestampFormat = "[HH:mm:ss] ";
            });
            builder.SetMinimumLevel(LogLevel.Information);
        });

        var fileReader = new FileReader(filePath, Encoding.UTF8);
        var subtitleParser = new SubtitleParser(loggerFactory.CreateLogger<SubtitleParser>());
        var fileLoader = new FileLoader(
            loggerFactory.CreateLogger<FileLoader>(),
            fileReader,
            subtitleParser
        );

        var subtitles = new List<ISubtitle>();

        await foreach (var subtitle in fileLoader.LoadFileAsync(filePath, degreeOfParallelism: 1))
        {
            subtitles.Add(subtitle);
        }

        Console.WriteLine("------ VÝPIS VŠECH TITULKŮ ------");
        foreach (var subtitle in subtitles)
        {
            Console.WriteLine(subtitle); // díky přepsané ToString() vypíše celý blok
            Console.WriteLine();
        }
    }
}

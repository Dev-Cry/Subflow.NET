# 🎬 Subflow.NET

**Subflow.NET** je moderní open-source knihovna v jazyce C# pro asynchronní načítání, čtení a robustní parsování titulků ve formátu `.srt`. Projekt je navržen s důrazem na čistotu kódu, výkonnostní efektivitu a snadnou rozšiřitelnost do budoucna (např. podpora více formátů).

---

## ✨ Funkce

- ✅ Asynchronní zpracování titulků pomocí `IAsyncEnumerable`
- ✅ Paralelní zpracování řádků přes `System.Threading.Tasks.Dataflow`
- ✅ Pokročilý parser `.srt` s validací a zotavením z běžných chyb
- ✅ Automatická korekce přehozených časových rozsahů
- ✅ Detekce nevalidních indexů nebo nekompletních bloků
- ✅ Logging přes `Microsoft.Extensions.Logging`
- ✅ Připraveno na DI a Clean Architecture
- ✅ Kompatibilní s .NET 6, .NET 7 a .NET 8

---

## 🚀 Ukázka použití

```csharp
var fileReader = new FileReader("soubor.srt", Encoding.UTF8);
var parser = new SubtitleParser(logger);
var loader = new FileLoader(logger, fileReader, parser);

await foreach (var subtitle in loader.LoadFileAsync("soubor.srt"))
{
    Console.WriteLine(subtitle.ToString());
}

## 📦 Instalace

Zatím není dostupné jako NuGet balíček – ručně přidej projekt jako submodul nebo přímou referenci do svého řešení:

```bash
git submodule add https://github.com/tvoje-uzivatelske-jmeno/Subflow.NET.git


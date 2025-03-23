# 🎬 Subflow.NET

**Subflow.NET** je moderní .NET knihovna pro asynchronní načítání, čtení a parsování titulků (např. `.srt`) s důrazem na rozšiřitelnost, přesnost a výkon. Je navržena s využitím čistých architektonických principů, asynchronního zpracování a pokročilých nástrojů .NET jako `System.Threading.Tasks.Dataflow`.

---

## ✨ Hlavní vlastnosti

- ✅ Asynchronní čtení a parsování titulků
- ✅ Podpora formátu `.srt` (SubRip)
- ✅ Detekce a korekce běžných chyb ve formátu
- ✅ Paralelní zpracování řádků s možností nastavení stupně paralelismu
- ✅ Jednoduchá integrace pomocí DI (dependency injection)
- ✅ Připraveno na rozšíření pro další formáty (.vtt, .ass, ...)

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

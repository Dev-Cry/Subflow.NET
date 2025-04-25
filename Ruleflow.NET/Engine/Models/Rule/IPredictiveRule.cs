using Ruleflow.NET.Engine.Models.Rule.Interface;

/// <summary>
/// Rozhraní pro prediktivní pravidlo, které využívá historická data pro predikci.
/// </summary>
/// <typeparam name="TInput">Typ validovaných dat.</typeparam>
/// <typeparam name="THistoryData">Typ historických dat používaných pro predikci.</typeparam>
public interface IPredictiveRule<TInput, THistoryData> : IRule<TInput>
{
    /// <summary>
    /// Historická data používaná pro predikci.
    /// </summary>
    IReadOnlyList<THistoryData> HistoricalData { get; }

    /// <summary>
    /// Přidá historická data pro predikci.
    /// </summary>
    /// <param name="data">Historická data.</param>
    void AddHistoricalData(THistoryData data);

    /// <summary>
    /// Přidá kolekci historických dat pro predikci.
    /// </summary>
    /// <param name="data">Kolekce historických dat.</param>
    void AddHistoricalDataRange(IEnumerable<THistoryData> data);

    /// <summary>
    /// Vyčistí všechna historická data.
    /// </summary>
    void ClearHistoricalData();
}
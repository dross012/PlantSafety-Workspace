// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Application-layer facade composing the KPI engine domain calculators
// (Phase 5, Section 2.3) into a single entry point for computing the full production KPI
// suite (OEE, MTBF, MTTR, Throughput, Yield, Scrap Rate) for a measurement period.
// SchemaVersion: 1

using PlantSafety.Domain.Analytics.Dashboard.Kpi.Services;
using PlantSafety.Domain.Analytics.Dashboard.Kpi.ValueObjects;

namespace PlantSafety.Application.Analytics.Dashboard.Kpi;

/// <summary>Immutable snapshot of every platform-standard production KPI for a measurement period.</summary>
public sealed record ProductionKpiSnapshot
{
    /// <summary>Overall Equipment Effectiveness result.</summary>
    public required OeeResult Oee { get; init; }

    /// <summary>Mean Time Between Failures.</summary>
    public required TimeSpan Mtbf { get; init; }

    /// <summary>Mean Time To Repair.</summary>
    public required TimeSpan Mttr { get; init; }

    /// <summary>Throughput in units per hour.</summary>
    public required double ThroughputUnitsPerHour { get; init; }

    /// <summary>Yield ratio (0.0-1.0).</summary>
    public required double Yield { get; init; }

    /// <summary>Scrap rate ratio (0.0-1.0).</summary>
    public required double ScrapRate { get; init; }
}

/// <summary>
/// Facade composing the Section 2.3 KPI engine domain calculators into a single entry point,
/// consumed by Analytics.Dashboard and Analytics.ScoreCards to publish the production KPI suite.
/// </summary>
public sealed class ProductionKpiEngine
{
    private readonly OeeCalculator _oeeCalculator;
    private readonly MtbfCalculator _mtbfCalculator;
    private readonly MttrCalculator _mttrCalculator;
    private readonly ThroughputCalculator _throughputCalculator;
    private readonly YieldCalculator _yieldCalculator;
    private readonly ScrapRateCalculator _scrapRateCalculator;

    /// <summary>Creates the engine with the platform-default calculators.</summary>
    public ProductionKpiEngine()
        : this(new OeeCalculator(), new MtbfCalculator(), new MttrCalculator(),
               new ThroughputCalculator(), new YieldCalculator(), new ScrapRateCalculator()) { }

    /// <summary>Creates the engine with explicit calculator collaborators (for testing/composition).</summary>
    public ProductionKpiEngine(
        OeeCalculator oeeCalculator,
        MtbfCalculator mtbfCalculator,
        MttrCalculator mttrCalculator,
        ThroughputCalculator throughputCalculator,
        YieldCalculator yieldCalculator,
        ScrapRateCalculator scrapRateCalculator)
    {
        _oeeCalculator = oeeCalculator;
        _mtbfCalculator = mtbfCalculator;
        _mttrCalculator = mttrCalculator;
        _throughputCalculator = throughputCalculator;
        _yieldCalculator = yieldCalculator;
        _scrapRateCalculator = scrapRateCalculator;
    }

    /// <summary>Computes the full production KPI suite for a measurement period.</summary>
    public ProductionKpiSnapshot ComputeSnapshot(
        OeeInputs oeeInputs,
        MtbfInputs mtbfInputs,
        MttrInputs mttrInputs,
        ThroughputInputs throughputInputs,
        ProductionQualityInputs qualityInputs)
    {
        ArgumentNullException.ThrowIfNull(oeeInputs);
        ArgumentNullException.ThrowIfNull(mtbfInputs);
        ArgumentNullException.ThrowIfNull(mttrInputs);
        ArgumentNullException.ThrowIfNull(throughputInputs);
        ArgumentNullException.ThrowIfNull(qualityInputs);

        return new ProductionKpiSnapshot
        {
            Oee = _oeeCalculator.Calculate(oeeInputs),
            Mtbf = _mtbfCalculator.Calculate(mtbfInputs),
            Mttr = _mttrCalculator.Calculate(mttrInputs),
            ThroughputUnitsPerHour = _throughputCalculator.CalculateUnitsPerHour(throughputInputs),
            Yield = _yieldCalculator.Calculate(qualityInputs),
            ScrapRate = _scrapRateCalculator.Calculate(qualityInputs),
        };
    }
}

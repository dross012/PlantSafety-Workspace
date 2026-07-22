// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Scorecards
//  Layer:          Domain
//  File:           KpiMeasurement.cs
//  Created:        2026-07-21
//  Author:         PlantSafety Platform Team (Claude Code modular-creation-agent)
//  SchemaVersion:  2026.1
//  Description:    Child entity of the Scorecard aggregate representing a single computed KPI
//                  measurement for the scorecard's reporting period. Grounded in "Scorecard Data
//                  Sources": "Scorecards pull data from multiple PlantSafety modules" (Incident
//                  Management, Training, Auditing, Machines, Procedures, Contractors,
//                  Observability, Risk Management), each contributing the raw numerator/
//                  denominator inputs to a KpiMetricDefinition formula.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Scorecards.Enums;
using PlantSafety.Domain.Scorecards.ValueObjects.Ids;

namespace PlantSafety.Domain.Scorecards.Entities;

/// <summary>A single computed KPI measurement recorded against a Scorecard's reporting period.</summary>
public sealed class KpiMeasurement
{
    /// <summary>Unique identity of this measurement.</summary>
    public KpiMeasurementId KpiMeasurementId { get; private set; }

    /// <summary>Identity of the parent Scorecard aggregate.</summary>
    public ScorecardId ScorecardId { get; private set; }

    /// <summary>The canonical KPI this measurement reports against.</summary>
    public KpiMetricDefinition Metric { get; private set; }

    /// <summary>Raw numerator input sourced from the owning module (see Scorecard Data Sources).</summary>
    public decimal Numerator { get; private set; }

    /// <summary>Raw denominator input sourced from the owning module. Ignored for count-style metrics.</summary>
    public decimal Denominator { get; private set; }

    /// <summary>Computed metric value, per <see cref="KpiMetricDefinition.ComputeValue"/>.</summary>
    public decimal ComputedValue { get; private set; }

    /// <summary>Name of the source PlantSafety module that supplied the raw data (per Scorecard Data Sources).</summary>
    public string DataSourceModule { get; private set; } = string.Empty;

    /// <summary>UTC instant this measurement was recorded.</summary>
    public UtcTimestamp RecordedAtUtc { get; private set; }

    /// <summary>For ORM rehydration only.</summary>
    private KpiMeasurement() { }

    /// <summary>Creates a new KPI measurement, computing its value via the metric's canonical formula.</summary>
    public KpiMeasurement(
        ScorecardId scorecardId,
        KpiMetricDefinition metric,
        decimal numerator,
        decimal denominator,
        string dataSourceModule,
        UtcTimestamp recordedAtUtc)
    {
        KpiMeasurementId = KpiMeasurementId.New();
        ScorecardId = scorecardId;
        Metric = metric;
        Numerator = numerator;
        Denominator = denominator;
        ComputedValue = metric.ComputeValue(numerator, denominator);
        DataSourceModule = (dataSourceModule ?? string.Empty).Trim();
        RecordedAtUtc = recordedAtUtc;
    }
}

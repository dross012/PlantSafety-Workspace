// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Domain
//  File:           AdvancedMetricsEvents.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Domain events raised across the AdvancedMetricsLedger and EsgDisclosureReport
//                  aggregates: carbon/energy/water/waste recording, target setting and at-risk
//                  detection, and disclosure report generation. Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Sustainability.AdvancedMetrics.Events;

/// <summary>Raised when a Scope 1/2/3 carbon emission record is recorded on a ledger.</summary>
public sealed record CarbonEmissionRecordedEvent(
    AdvancedMetricsEventId EventId,
    AdvancedMetricsLedgerId AdvancedMetricsLedgerId,
    CarbonEmissionRecordId CarbonEmissionRecordId,
    GhgScope Scope,
    decimal EmissionsMetricTonsCo2e,
    string MethodologyVersion,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAdvancedMetricsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "CarbonEmissionRecorded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when an energy intensity metric is recorded on a ledger.</summary>
public sealed record EnergyIntensityMetricRecordedEvent(
    AdvancedMetricsEventId EventId,
    AdvancedMetricsLedgerId AdvancedMetricsLedgerId,
    EnergyIntensityMetricId EnergyIntensityMetricId,
    decimal IntensityValue,
    string MethodologyVersion,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAdvancedMetricsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EnergyIntensityMetricRecorded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a water intensity metric is recorded on a ledger.</summary>
public sealed record WaterIntensityMetricRecordedEvent(
    AdvancedMetricsEventId EventId,
    AdvancedMetricsLedgerId AdvancedMetricsLedgerId,
    WaterIntensityMetricId WaterIntensityMetricId,
    decimal IntensityValue,
    string MethodologyVersion,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAdvancedMetricsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "WaterIntensityMetricRecorded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a waste diversion record is recorded on a ledger.</summary>
public sealed record WasteDiversionRecordedEvent(
    AdvancedMetricsEventId EventId,
    AdvancedMetricsLedgerId AdvancedMetricsLedgerId,
    WasteDiversionRecordId WasteDiversionRecordId,
    decimal DiversionRatePercent,
    string MethodologyVersion,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAdvancedMetricsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "WasteDiversionRecorded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a sustainability target is set (or re-baselined) on a ledger.</summary>
public sealed record SustainabilityTargetSetEvent(
    AdvancedMetricsEventId EventId,
    AdvancedMetricsLedgerId AdvancedMetricsLedgerId,
    SustainabilityTargetId SustainabilityTargetId,
    SustainabilityTargetType TargetType,
    decimal TargetValue,
    UtcTimestamp TargetDateUtc,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAdvancedMetricsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "SustainabilityTargetSet";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>
/// Raised when a sustainability target's tracked progress trends behind plan. Consumed by
/// RiskManagement as a risk-hook trigger.
/// </summary>
public sealed record SustainabilityTargetAtRiskEvent(
    AdvancedMetricsEventId EventId,
    AdvancedMetricsLedgerId AdvancedMetricsLedgerId,
    SustainabilityTargetId SustainabilityTargetId,
    SustainabilityTargetType TargetType,
    decimal CurrentValue,
    decimal TargetValue,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAdvancedMetricsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "SustainabilityTargetAtRisk";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a new (draft) ESG disclosure report is generated.</summary>
public sealed record EsgDisclosureReportGeneratedEvent(
    AdvancedMetricsEventId EventId,
    EsgDisclosureReportId EsgDisclosureReportId,
    int Version,
    EsgDisclosureFramework Frameworks,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAdvancedMetricsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EsgDisclosureReportGenerated";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>Raised when a draft ESG disclosure report is published and becomes immutable.</summary>
public sealed record EsgDisclosureReportPublishedEvent(
    AdvancedMetricsEventId EventId,
    EsgDisclosureReportId EsgDisclosureReportId,
    int Version,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAdvancedMetricsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EsgDisclosureReportPublished";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

/// <summary>
/// Raised when a published ESG disclosure report is superseded by a newer versioned report
/// (data-governance correction path -- published reports are never mutated in place).
/// </summary>
public sealed record EsgDisclosureReportSupersededEvent(
    AdvancedMetricsEventId EventId,
    EsgDisclosureReportId EsgDisclosureReportId,
    EsgDisclosureReportId SupersededByReportId,
    UtcTimestamp OccurredAtUtc,
    PerformedByActor Actor,
    CorrelationId CorrelationId,
    CausationId CausationId
) : IAdvancedMetricsDomainEvent
{
    /// <inheritdoc/>
    public string EventType => "EsgDisclosureReportSuperseded";
    /// <inheritdoc/>
    public int EventVersion => 1;
    /// <inheritdoc/>
    public string SchemaVersion => "2026.1";
}

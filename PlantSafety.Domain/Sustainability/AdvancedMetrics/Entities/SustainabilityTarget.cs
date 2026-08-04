// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Domain
//  File:           SustainabilityTarget.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Mutable child entity of AdvancedMetricsLedger tracking a single KPI target
//                  (e.g. carbon intensity, energy intensity, water intensity, waste diversion rate)
//                  and its progress status. Progress is evaluated by SustainabilityTargetRiskPolicy.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Sustainability.AdvancedMetrics.Entities;

/// <summary>A single KPI target and its tracked progress on an <see cref="Aggregates.AdvancedMetricsLedger"/>.</summary>
public sealed class SustainabilityTarget
{
    /// <summary>Unique identity of this target.</summary>
    public SustainabilityTargetId SustainabilityTargetId { get; private set; }

    /// <summary>The KPI this target tracks.</summary>
    public SustainabilityTargetType TargetType { get; private set; }

    /// <summary>Baseline value the target is measured against.</summary>
    public decimal BaselineValue { get; private set; }

    /// <summary>Target value to be reached by <see cref="TargetDateUtc"/>.</summary>
    public decimal TargetValue { get; private set; }

    /// <summary>Most recently observed value for the tracked KPI.</summary>
    public decimal CurrentValue { get; private set; }

    /// <summary>Date by which the target must be reached.</summary>
    public UtcTimestamp TargetDateUtc { get; private set; }

    /// <summary>Current progress status.</summary>
    public SustainabilityTargetStatus Status { get; private set; }

    /// <summary>UTC instant the target was set.</summary>
    public UtcTimestamp SetAtUtc { get; private set; }

    /// <summary>Identity of the user who set this target.</summary>
    public UserId SetBy { get; private set; }

    /// <summary>UTC instant of the most recent progress update, if any.</summary>
    public UtcTimestamp? LastProgressUpdateAtUtc { get; private set; }

    /// <summary>For ORM rehydration only.</summary>
    private SustainabilityTarget() { }

    /// <summary>Creates a new sustainability target.</summary>
    public SustainabilityTarget(
        SustainabilityTargetId sustainabilityTargetId,
        SustainabilityTargetType targetType,
        decimal baselineValue,
        decimal targetValue,
        UtcTimestamp targetDateUtc,
        UtcTimestamp setAtUtc,
        UserId setBy)
    {
        SustainabilityTargetId = sustainabilityTargetId;
        TargetType              = targetType;
        BaselineValue           = baselineValue;
        TargetValue             = targetValue;
        CurrentValue            = baselineValue;
        TargetDateUtc           = targetDateUtc;
        Status                  = SustainabilityTargetStatus.OnTrack;
        SetAtUtc                = setAtUtc;
        SetBy                   = setBy;
    }

    /// <summary>Updates the tracked current value and recomputes progress status.</summary>
    /// <returns><see langword="true"/> when this update transitioned the target into <see cref="SustainabilityTargetStatus.AtRisk"/>.</returns>
    internal bool UpdateProgress(decimal currentValue, SustainabilityTargetStatus newStatus, UtcTimestamp atUtc)
    {
        var wasAtRisk = Status == SustainabilityTargetStatus.AtRisk;

        CurrentValue             = currentValue;
        Status                   = newStatus;
        LastProgressUpdateAtUtc  = atUtc;

        return !wasAtRisk && newStatus == SustainabilityTargetStatus.AtRisk;
    }
}

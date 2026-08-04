// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Domain
//  File:           CarbonEmissionRecord.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Immutable child entity of AdvancedMetricsLedger recording a single Scope 1/2/3
//                  GHG emissions activity. Carries the calculation methodology version alongside
//                  the value so historical reports built from it stay reproducible (data-governance
//                  requirement). Additive-only evolution.
// ============================================================================
namespace PlantSafety.Domain.Sustainability.AdvancedMetrics.Entities;

/// <summary>A single Scope 1/2/3 GHG emissions activity record on an <see cref="Aggregates.AdvancedMetricsLedger"/>.</summary>
public sealed class CarbonEmissionRecord
{
    /// <summary>Unique identity of this record.</summary>
    public CarbonEmissionRecordId CarbonEmissionRecordId { get; private set; }

    /// <summary>GHG Protocol scope classification.</summary>
    public GhgScope Scope { get; private set; }

    /// <summary>Emissions activity description (e.g. "Natural gas combustion - Boiler House 2").</summary>
    public string ActivityDescription { get; private set; } = string.Empty;

    /// <summary>Recorded emissions in metric tons of CO2-equivalent.</summary>
    public decimal EmissionsMetricTonsCo2e { get; private set; }

    /// <summary>Version identifier of the calculation methodology used to derive this value (e.g. "GHG-Protocol-2026.1").</summary>
    public string MethodologyVersion { get; private set; } = string.Empty;

    /// <summary>UTC instant the underlying emissions activity occurred.</summary>
    public UtcTimestamp ActivityAtUtc { get; private set; }

    /// <summary>UTC instant this record was captured in the ledger.</summary>
    public UtcTimestamp RecordedAtUtc { get; private set; }

    /// <summary>Identity of the user who recorded this entry.</summary>
    public UserId RecordedBy { get; private set; }

    /// <summary>For ORM rehydration only.</summary>
    private CarbonEmissionRecord() { }

    /// <summary>Creates a new carbon emission record.</summary>
    public CarbonEmissionRecord(
        CarbonEmissionRecordId carbonEmissionRecordId,
        GhgScope scope,
        string activityDescription,
        decimal emissionsMetricTonsCo2e,
        string methodologyVersion,
        UtcTimestamp activityAtUtc,
        UtcTimestamp recordedAtUtc,
        UserId recordedBy)
    {
        if (emissionsMetricTonsCo2e < 0)
            throw new ArgumentOutOfRangeException(nameof(emissionsMetricTonsCo2e), "Emissions value must not be negative.");
        if (string.IsNullOrWhiteSpace(methodologyVersion))
            throw new ArgumentException("MethodologyVersion must not be null or whitespace.", nameof(methodologyVersion));

        CarbonEmissionRecordId  = carbonEmissionRecordId;
        Scope                   = scope;
        ActivityDescription     = (activityDescription ?? string.Empty).Trim();
        EmissionsMetricTonsCo2e = emissionsMetricTonsCo2e;
        MethodologyVersion      = methodologyVersion.Trim();
        ActivityAtUtc           = activityAtUtc;
        RecordedAtUtc           = recordedAtUtc;
        RecordedBy              = recordedBy;
    }
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HazardousWasteManagement.WasteStreams
//  Layer:          Domain
//  File:           WasteStreamHistoryEntry.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.2
//  Description:    Immutable entity representing a single characterization version or lifecycle
//                  transition for a WasteStream aggregate. Supports "characterization data
//                  versioned not overwritten" data-governance requirement.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Domain.HazardousWasteManagement.WasteStreams.ValueObjects.Ids;

namespace PlantSafety.Domain.HazardousWasteManagement.WasteStreams.Entities;

/// <summary>Records a single characterization version or status transition on a WasteStream aggregate.</summary>
public sealed class WasteStreamHistoryEntry
{
    /// <summary>Unique identity of this history record.</summary>
    public Guid HistoryId { get; private set; }

    /// <summary>Identity of the parent WasteStream aggregate.</summary>
    public WasteStreamId WasteStreamId { get; private set; }

    /// <summary>Characterization version this entry captures.</summary>
    public int CharacterizationVersion { get; private set; }

    /// <summary>EPA waste code at this version.</summary>
    public string EpaWasteCode { get; private set; } = string.Empty;

    /// <summary>Hazard class at this version.</summary>
    public string HazardClass { get; private set; } = string.Empty;

    /// <summary>UTC instant this version was recorded.</summary>
    public UtcTimestamp ChangedAtUtc { get; private set; }

    /// <summary>Actor who recorded this version.</summary>
    public string ChangedBy { get; private set; } = string.Empty;

    /// <summary>For ORM rehydration only.</summary>
    private WasteStreamHistoryEntry() { }

    /// <summary>Creates a new immutable history entry for the given characterization version.</summary>
    public WasteStreamHistoryEntry(
        Guid historyId, WasteStreamId id, int characterizationVersion,
        string epaWasteCode, string hazardClass, UtcTimestamp changedAtUtc, string changedBy)
    {
        HistoryId               = historyId;
        WasteStreamId            = id;
        CharacterizationVersion = characterizationVersion;
        EpaWasteCode             = epaWasteCode;
        HazardClass              = hazardClass;
        ChangedAtUtc             = changedAtUtc;
        ChangedBy                = changedBy;
    }
}

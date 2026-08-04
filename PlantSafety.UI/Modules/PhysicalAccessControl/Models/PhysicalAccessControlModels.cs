// ============================================================
// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Client-side DTOs for the PhysicalAccessControl module, mirroring the
//   PlantSafety.Api read-model shapes for AccessPoints, Credentials, AccessEvents, SafetyZones,
//   and AccessAnomalies. Display only.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.PhysicalAccessControl.Models;

/// <summary>A single access-control device (badge reader, turnstile, mantrap door, or vehicle gate).</summary>
public sealed class AccessPointDto
{
    /// <summary>Gets the access point record identifier.</summary>
    public Guid AccessPointId { get; init; }

    /// <summary>Gets the access-point code/label.</summary>
    public string AccessPointCode { get; init; } = string.Empty;

    /// <summary>Gets the narrative description / physical location.</summary>
    public string? Description { get; init; }

    /// <summary>Gets the physical device classification.</summary>
    public string DeviceType { get; init; } = string.Empty;

    /// <summary>Gets the identity of the safety zone this access point guards, if any.</summary>
    public Guid? SafetyZoneId { get; init; }

    /// <summary>Gets whether this access point is on the site perimeter.</summary>
    public bool IsPerimeter { get; init; }

    /// <summary>Gets the overall status classification.</summary>
    public string Status { get; init; } = string.Empty;
}

/// <summary>Response payload for the access-point-by-id read-model query.</summary>
public sealed class GetAccessPointByIdResponseDto
{
    /// <summary>Gets the matching access point, or null if not found.</summary>
    public AccessPointDto? AccessPoint { get; init; }
}

/// <summary>Status projection for a single credential (badge) record. PII-minimal by design.</summary>
public sealed class CredentialStatusDto
{
    /// <summary>Gets the credential record identifier.</summary>
    public Guid CredentialId { get; init; }

    /// <summary>Gets the masked badge number (last 4 digits only).</summary>
    public string BadgeNumberMasked { get; init; } = string.Empty;

    /// <summary>Gets the classification of the holder.</summary>
    public string HolderType { get; init; } = string.Empty;

    /// <summary>Gets the access-level label granted by this credential.</summary>
    public string AccessLevel { get; init; } = string.Empty;

    /// <summary>Gets the current lifecycle status.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp the credential was issued.</summary>
    public DateTime IssuedAtUtc { get; init; }

    /// <summary>Gets the UTC timestamp the credential expires, if any.</summary>
    public DateTime? ExpiresAtUtc { get; init; }
}

/// <summary>Response payload for the credential-status read-model query.</summary>
public sealed class GetCredentialStatusResponseDto
{
    /// <summary>Gets the matching credential status projection, or null if not found.</summary>
    public CredentialStatusDto? Credential { get; init; }
}

/// <summary>A single ingested physical-access event.</summary>
public sealed class AccessEventDto
{
    /// <summary>Gets the access event record identifier.</summary>
    public Guid AccessEventId { get; init; }

    /// <summary>Gets the identity of the access point that produced this event.</summary>
    public Guid AccessPointId { get; init; }

    /// <summary>Gets the identity of the credential presented, if any.</summary>
    public Guid? CredentialId { get; init; }

    /// <summary>Gets the raw event outcome as reported by the badge system.</summary>
    public string EventType { get; init; } = string.Empty;

    /// <summary>Gets the direction of travel.</summary>
    public string Direction { get; init; } = string.Empty;

    /// <summary>Gets the processing status.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp the underlying physical event occurred.</summary>
    public DateTime OccurredAtUtc { get; init; }
}

/// <summary>Response payload for the access-events-by-zone read-model query.</summary>
public sealed class ListAccessEventsByZoneResponseDto
{
    /// <summary>Gets the matching access event records. Never null; empty when no results.</summary>
    public IReadOnlyList<AccessEventDto> Items { get; init; } = [];
}

/// <summary>Occupancy projection for a single safety zone, correlated from ingested access events.</summary>
public sealed class SafetyZoneOccupancyDto
{
    /// <summary>Gets the safety zone record identifier.</summary>
    public Guid SafetyZoneId { get; init; }

    /// <summary>Gets the zone code/label.</summary>
    public string ZoneCode { get; init; } = string.Empty;

    /// <summary>Gets the zone name.</summary>
    public string ZoneName { get; init; } = string.Empty;

    /// <summary>Gets the maximum permitted concurrent occupancy.</summary>
    public int MaxOccupancy { get; init; }

    /// <summary>Gets the current occupancy, as correlated from ingested access events.</summary>
    public int CurrentOccupancy { get; init; }

    /// <summary>Gets the overall zone status classification.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp this occupancy figure was last recalculated.</summary>
    public DateTime AsOfUtc { get; init; }
}

/// <summary>Response payload for the zone-occupancy read-model query.</summary>
public sealed class GetZoneOccupancyResponseDto
{
    /// <summary>Gets the matching zone occupancy projection, or null if not found.</summary>
    public SafetyZoneOccupancyDto? Occupancy { get; init; }
}

/// <summary>A single system-detected physical-access anomaly.</summary>
public sealed class AccessAnomalyDto
{
    /// <summary>Gets the anomaly record identifier.</summary>
    public Guid AccessAnomalyId { get; init; }

    /// <summary>Gets the identity of the access point where the anomaly occurred.</summary>
    public Guid AccessPointId { get; init; }

    /// <summary>Gets the identity of the safety zone affected, if known.</summary>
    public Guid? SafetyZoneId { get; init; }

    /// <summary>Gets the classification of the anomaly.</summary>
    public string AnomalyType { get; init; } = string.Empty;

    /// <summary>Gets the severity score (0-100).</summary>
    public decimal SeverityScore { get; init; }

    /// <summary>Gets the current triage status.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp the anomaly was detected.</summary>
    public DateTime CreatedAtUtc { get; init; }
}

/// <summary>Response payload for the active-anomalies read-model query.</summary>
public sealed class ListActiveAnomaliesResponseDto
{
    /// <summary>Gets the matching active anomaly records. Never null; empty when no results.</summary>
    public IReadOnlyList<AccessAnomalyDto> Items { get; init; } = [];
}

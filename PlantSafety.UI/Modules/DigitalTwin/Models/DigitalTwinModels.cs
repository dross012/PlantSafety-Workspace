// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTOs for the Digital Twin module. No
//   PlantSafety.Api read-model endpoint exists yet for this module today —
//   PlantSafety.Domain/DigitalTwin is a thin Id/CreatedAtUtc/CreatedBy
//   scaffold aggregate with no queries/handlers/controllers wired. This
//   shape is the best-inferable read-model DTO (asset/twin registry entry)
//   following the sibling Intelligence read-model conventions
//   (TenantId/SiteId/GeneratedAt/Items envelope) so the UI is ready to bind
//   the instant a real endpoint ships. Until then, AiTwinApiClient calls
//   gracefully fault to the page's Error state — see DigitalTwinViewerPage.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.DigitalTwin.Models;

/// <summary>Represents a single digital twin asset registry entry.</summary>
public sealed class DigitalTwinAssetDto
{
    /// <summary>Gets the unique identifier of the digital twin asset.</summary>
    public Guid TwinId { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the display name of the physical asset this twin represents.</summary>
    public string AssetName { get; init; } = string.Empty;

    /// <summary>Gets the asset type/category (e.g., Machine, Line, Facility).</summary>
    public string AssetType { get; init; } = string.Empty;

    /// <summary>Gets the current twin synchronization status (e.g., Synced/Degraded/Offline).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp of the twin's last successful telemetry sync.</summary>
    public DateTime LastSyncAtUtc { get; init; }

    /// <summary>Gets the telemetry health score (0.0-1.0) for this twin.</summary>
    public double TelemetryHealthScore { get; init; }
}

/// <summary>Envelope for the (not-yet-wired) GET v1/digital-twin/twins endpoint.</summary>
public sealed class DigitalTwinResponseDto
{
    /// <summary>Gets the tenant the data belongs to.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site the data is scoped to, or null for tenant-wide results.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the UTC timestamp when this read-model snapshot was generated.</summary>
    public DateTime GeneratedAt { get; init; }

    /// <summary>Gets the digital twin asset items.</summary>
    public IReadOnlyList<DigitalTwinAssetDto> Items { get; init; } = [];
}

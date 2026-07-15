// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTOs for the Ergonomics module. Mirrors
//   PlantSafety.Application.Operational.Ergonomics.Models.ErgonomicsItemDto
//   and the ErgonomicsResponse contract returned by GET /v1/operational/ergonomics
//   (tenant/site/date-range scoped read-model — no pagination, no single-record
//   GetById endpoint).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.Ergonomics.Models;

/// <summary>A single Ergonomics read-model record.</summary>
public sealed class ErgonomicsItemDto
{
    /// <summary>Gets the ergonomics assessment identifier.</summary>
    public Guid ErgonomicsAssessmentId { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the identifier of the workstation assessed.</summary>
    public Guid WorkstationId { get; init; }

    /// <summary>Gets the risk level classification (e.g., Low/Medium/High).</summary>
    public string RiskLevel { get; init; } = string.Empty;

    /// <summary>Gets the current status of this record.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp when this record was last actioned.</summary>
    public DateTime AssessedAtUtc { get; init; }

    /// <summary>Gets the identity of the user who last actioned this record.</summary>
    public Guid AssessedBy { get; init; }
}

/// <summary>Response envelope for GET /v1/operational/ergonomics.</summary>
public sealed class ErgonomicsResponseDto
{
    /// <summary>Gets the tenant the data belongs to.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site the data is scoped to, or null for tenant-wide results.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the UTC timestamp when this read-model snapshot was generated.</summary>
    public DateTime GeneratedAt { get; init; }

    /// <summary>Gets the result items from the Ergonomics read-model.</summary>
    public IReadOnlyList<ErgonomicsItemDto> Items { get; init; } = [];
}

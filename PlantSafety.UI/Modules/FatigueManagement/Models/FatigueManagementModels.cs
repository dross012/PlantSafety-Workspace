// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTOs for the Fatigue Management module.
//   Mirrors PlantSafety.Application.Operational.FatigueManagement.Models
//   .FatigueManagementItemDto and the FatigueManagementResponse contract
//   returned by GET /v1/operational/fatigue-management (tenant/site/date-range
//   scoped read-model — no pagination, no single-record GetById endpoint).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.FatigueManagement.Models;

/// <summary>A single Fatigue Management read-model record.</summary>
public sealed class FatigueManagementItemDto
{
    /// <summary>Gets the fatigue record identifier.</summary>
    public Guid FatigueRecordId { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the employee this fatigue record relates to.</summary>
    public Guid EmployeeId { get; init; }

    /// <summary>Gets the fatigue risk level classification.</summary>
    public string FatigueRiskLevel { get; init; } = string.Empty;

    /// <summary>Gets the current status of this record.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp when this record was last actioned.</summary>
    public DateTime RecordedAtUtc { get; init; }

    /// <summary>Gets the identity of the user who last actioned this record.</summary>
    public Guid RecordedBy { get; init; }
}

/// <summary>Response envelope for GET /v1/operational/fatigue-management.</summary>
public sealed class FatigueManagementResponseDto
{
    /// <summary>Gets the tenant the data belongs to.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site the data is scoped to, or null for tenant-wide results.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the UTC timestamp when this read-model snapshot was generated.</summary>
    public DateTime GeneratedAt { get; init; }

    /// <summary>Gets the result items from the Fatigue Management read-model.</summary>
    public IReadOnlyList<FatigueManagementItemDto> Items { get; init; } = [];
}

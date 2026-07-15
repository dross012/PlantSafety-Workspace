// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTOs mirroring the Contractor read-model
//   endpoint (GET v1/operational/contractor). Deserialized directly into
//   the inner Data shape of the ApiResponse&lt;T&gt; envelope, matching the
//   established sibling convention (see
//   Modules/PlanningProjects/Models/PlanningProjectsListResponseDto.cs).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.ContractorManagement.Models;

/// <summary>A single Contractor record (GET v1/operational/contractor).</summary>
public sealed class ContractorItemDto
{
    /// <summary>Contractor record identifier.</summary>
    public Guid ContractorRecordId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Name of the contractor organisation.</summary>
    public string ContractorName { get; init; } = string.Empty;

    /// <summary>Type of service the contractor provides.</summary>
    public string ServiceType { get; init; } = string.Empty;

    /// <summary>Current status of this record.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>UTC timestamp when this record was last actioned.</summary>
    public DateTime OnboardedAtUtc { get; init; }

    /// <summary>Identity of the user who last actioned this record.</summary>
    public Guid ApprovedBy { get; init; }
}

/// <summary>Response payload for the Contractor read-model list endpoint.</summary>
public sealed class ContractorResponseDto
{
    /// <summary>Gets the tenant the data belongs to.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site the data is scoped to, or null for tenant-wide results.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the UTC timestamp when this read-model snapshot was generated.</summary>
    public DateTime GeneratedAt { get; init; }

    /// <summary>Gets the matching contractor items. Never null; empty when no results.</summary>
    public IReadOnlyList<ContractorItemDto> Items { get; init; } = [];
}

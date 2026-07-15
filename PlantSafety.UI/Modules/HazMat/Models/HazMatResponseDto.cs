// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTO mirroring PlantSafety.Api.Operational.
//   HazardousMaterials.Contracts.Responses.HazardousMaterialsResponse — the
//   payload nested inside the server's ApiResponse&lt;T&gt; envelope for
//   GET api/v1/operational/hazardous-materials.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.HazMat.Models;

/// <summary>Response payload for HazMat read-model queries.</summary>
public sealed class HazMatResponseDto
{
    /// <summary>Gets the tenant the data belongs to.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site the data is scoped to, or null for tenant-wide results.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the UTC timestamp when this read-model snapshot was generated.</summary>
    public DateTime GeneratedAt { get; init; }

    /// <summary>Gets the matching HazMat records. Never null; empty when no results.</summary>
    public IReadOnlyList<HazMatDto> Items { get; init; } = [];
}

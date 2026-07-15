// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTO mirroring PlantSafety.Api.Operational.
//   LockoutTagout.Contracts.Responses.LockoutTagoutResponse — the payload
//   nested inside the server's ApiResponse&lt;T&gt; envelope for
//   GET api/v1/operational/lockout-tagout.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.Loto.Models;

/// <summary>Response payload for LOTO read-model queries.</summary>
public sealed class LotoResponseDto
{
    /// <summary>Gets the tenant the data belongs to.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site the data is scoped to, or null for tenant-wide results.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the UTC timestamp when this read-model snapshot was generated.</summary>
    public DateTime GeneratedAt { get; init; }

    /// <summary>Gets the matching LOTO records. Never null; empty when no results.</summary>
    public IReadOnlyList<LotoDto> Items { get; init; } = [];
}

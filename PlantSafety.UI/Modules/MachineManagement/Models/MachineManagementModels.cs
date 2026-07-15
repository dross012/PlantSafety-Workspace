// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTOs for the Machine Management module. Backed
//   by the PlantSafety.Api CrossDomain Machines read-model
//   (api/v1/cross-domain/machines), which wraps its payload in the
//   platform-wide ApiResponse<T> shape (Success/Data/Errors/Metadata).
//   ReadModelEnvelopeDto/ReadModelPayloadDto mirror that wire shape exactly
//   so SafetyApiClientBase.GetAsync<T> can deserialize it directly. No domain
//   logic present — display only.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.MachineManagement.Models;

/// <summary>A single machine/equipment record from the Machines read-model.</summary>
public sealed record MachineItemDto
{
    /// <summary>Machine identifier.</summary>
    public Guid MachineId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Machine display name.</summary>
    public string MachineName { get; init; } = string.Empty;

    /// <summary>Machine type/class (e.g., Press/Conveyor/CNC).</summary>
    public string MachineType { get; init; } = string.Empty;

    /// <summary>Manufacturer serial number.</summary>
    public string SerialNumber { get; init; } = string.Empty;

    /// <summary>Current machine status (e.g., Operational/Maintenance/OutOfService).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>UTC timestamp of the machine's last service.</summary>
    public DateTime LastServicedAtUtc { get; init; }

    /// <summary>Identity of the technician who last serviced the machine.</summary>
    public Guid LastServicedBy { get; init; }
}

/// <summary>Read-model payload envelope, matching PlantSafety.Api's per-endpoint Response records.</summary>
/// <typeparam name="TItem">The item type contained in <see cref="Items"/>.</typeparam>
public sealed record ReadModelPayloadDto<TItem>
{
    /// <summary>Tenant the data belongs to.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Site the data is scoped to, or null for tenant-wide results.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>UTC timestamp when this read-model snapshot was generated.</summary>
    public DateTime GeneratedAt { get; init; }

    /// <summary>Result items. Never null; empty when no results.</summary>
    public IReadOnlyList<TItem> Items { get; init; } = [];
}

/// <summary>
/// Client-side mirror of PlantSafety.Api.Models.ApiResponse&lt;T&gt; — the platform-wide
/// controller envelope (Success/Data/Errors/Metadata) used by all Compliance/Operational/
/// CrossDomain/Analytics read-model endpoints.
/// </summary>
/// <typeparam name="TItem">The item type contained in the payload's <c>Items</c> list.</typeparam>
public sealed record ReadModelEnvelopeDto<TItem>
{
    /// <summary>Gets a value indicating whether the API operation succeeded.</summary>
    public bool Success { get; init; }

    /// <summary>Gets the payload. Null when <see cref="Success"/> is false.</summary>
    public ReadModelPayloadDto<TItem>? Data { get; init; }

    /// <summary>Gets human-readable error messages. Empty on success.</summary>
    public IReadOnlyList<string> Errors { get; init; } = [];
}

// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTOs for the Worker Management module. Backed
//   by the PlantSafety.Api CrossDomain Employees read-model
//   (api/v1/cross-domain/employees), which wraps its payload in the
//   platform-wide ApiResponse<T> shape (Success/Data/Errors/Metadata) rather
//   than the Alerts-style bare-body envelope — ReadModelEnvelopeDto/
//   ReadModelPayloadDto mirror that wire shape exactly so
//   SafetyApiClientBase.GetAsync<T> can deserialize it directly. No domain
//   logic present — display only.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.WorkerManagement.Models;

/// <summary>A single worker/employee record from the Employees read-model.</summary>
public sealed record EmployeeItemDto
{
    /// <summary>Employee identifier.</summary>
    public Guid EmployeeId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Employee full name.</summary>
    public string FullName { get; init; } = string.Empty;

    /// <summary>Employee number/badge identifier.</summary>
    public string EmployeeNumber { get; init; } = string.Empty;

    /// <summary>Department the employee belongs to.</summary>
    public string Department { get; init; } = string.Empty;

    /// <summary>Current employment status (e.g., Active/OnLeave/Terminated).</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>UTC date the employee was hired.</summary>
    public DateTime HireDateUtc { get; init; }

    /// <summary>Identity of the employee's assigned manager.</summary>
    public Guid ManagerId { get; init; }
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
/// CrossDomain/Analytics read-model endpoints. Distinct from
/// <see cref="PlantSafety.UI.Models.Common.ApiResponseEnvelope{T}"/>, which models the
/// HTTP-transport-level outcome (success/problem/fault); this type models the JSON body
/// itself once the HTTP call has already succeeded.
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

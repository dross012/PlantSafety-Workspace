// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTOs for the Training module. Backed by the
//   PlantSafety.Api Operational Training read-model
//   (api/v1/operational/training), which wraps its payload in the
//   platform-wide ApiResponse<T> shape (Success/Data/Errors/Metadata).
//   ReadModelEnvelopeDto/ReadModelPayloadDto mirror that wire shape exactly
//   so SafetyApiClientBase.GetAsync<T> can deserialize it directly. No domain
//   logic present — display only.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.Training.Models;

/// <summary>A single training completion/assignment record.</summary>
public sealed record TrainingRecordItemDto
{
    /// <summary>Training record identifier.</summary>
    public Guid TrainingRecordId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Employee this training record relates to.</summary>
    public Guid EmployeeId { get; init; }

    /// <summary>Name of the training course.</summary>
    public string CourseName { get; init; } = string.Empty;

    /// <summary>Current status of this record.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>UTC timestamp when this record was last actioned (completed).</summary>
    public DateTime CompletedAtUtc { get; init; }

    /// <summary>Identity of the user who last actioned (certified) this record.</summary>
    public Guid CertifiedBy { get; init; }
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

// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Client-side DTOs for the Industrial Hygiene module.
//   Mirrors PlantSafety.Application.Operational.IndustrialHygiene.Models
//   .IndustrialHygieneItemDto and the IndustrialHygieneResponse contract
//   returned by GET /v1/operational/industrial-hygiene (tenant/site/date-range
//   scoped read-model — no pagination, no single-record GetById endpoint).
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.4
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.IndustrialHygiene.Models;

/// <summary>A single Industrial Hygiene read-model record.</summary>
public sealed class IndustrialHygieneItemDto
{
    /// <summary>Gets the industrial hygiene sample identifier.</summary>
    public Guid IndustrialHygieneSampleId { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the type of industrial hygiene sample collected.</summary>
    public string SampleType { get; init; } = string.Empty;

    /// <summary>Gets the exposure level classification.</summary>
    public string ExposureLevel { get; init; } = string.Empty;

    /// <summary>Gets the current status of this record.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp when this record was last actioned.</summary>
    public DateTime SampledAtUtc { get; init; }

    /// <summary>Gets the identity of the user who last actioned this record.</summary>
    public Guid SampledBy { get; init; }
}

/// <summary>Response envelope for GET /v1/operational/industrial-hygiene.</summary>
public sealed class IndustrialHygieneResponseDto
{
    /// <summary>Gets the tenant the data belongs to.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the site the data is scoped to, or null for tenant-wide results.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Gets the UTC timestamp when this read-model snapshot was generated.</summary>
    public DateTime GeneratedAt { get; init; }

    /// <summary>Gets the result items from the Industrial Hygiene read-model.</summary>
    public IReadOnlyList<IndustrialHygieneItemDto> Items { get; init; } = [];
}

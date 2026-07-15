// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-07-14
// FILE DESCRIPTION: Display-ready view model for a single Contractor
//   record, used by both the list and detail pages.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;

namespace PlantSafety.UI.Modules.ContractorManagement.Models;

/// <summary>List-row and detail view model for a single Contractor record.</summary>
public sealed class ContractorViewModel
{
    /// <summary>Gets the contractor record identifier.</summary>
    public required Guid ContractorRecordId { get; init; }

    /// <summary>Gets the contractor organisation name.</summary>
    public required string ContractorName { get; init; }

    /// <summary>Gets the type of service the contractor provides.</summary>
    public required string ServiceType { get; init; }

    /// <summary>Gets the raw status code.</summary>
    public required string Status { get; init; }

    /// <summary>Gets the StatusIndicatorComponent operational state derived from <see cref="Status"/>.</summary>
    public required string StatusIndicatorState { get; init; }

    /// <summary>Gets the UTC timestamp when this record was last actioned.</summary>
    public required DateTime OnboardedAtUtc { get; init; }

    /// <summary>Gets the identity of the user who last actioned this record.</summary>
    public required Guid ApprovedBy { get; init; }

    /// <summary>Gets the tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Gets the site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Financials
//  Layer:          Application
//  File:           FinancialRecordDetailDto.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Flat read-model item DTO for a single FinancialRecord, including every
//                  aggregate property, its line items, and an ETag. Backs
//                  FinancialRecordByIdQuery. Distinct from the pre-existing, simpler
//                  FinancialsItemDto used by the date-range list query (GetFinancialsQuery), which
//                  is left entirely untouched by this pass. Named "...DetailDto" rather than the
//                  "<Name>ItemDto" convention used elsewhere in this Phase 4 pass to mirror the
//                  identical precedent already established by
//                  PlantSafety.Application.ContinuousImprovement.RCA.Models.RCADetailDto for the
//                  sibling ContinuousImprovement.RCA "leave the existing read stack untouched,
//                  build a parallel by-id path" build-out.
//                  Additive-only evolution.
// ============================================================================
namespace PlantSafety.Application.Financials.Models;

/// <summary>A single line item within a FinancialRecord read-model record.</summary>
public sealed record FinancialRecordLineItemDto
{
    /// <summary>Line item identifier.</summary>
    public required Guid LineItemId { get; init; }

    /// <summary>Description of the line item.</summary>
    public required string Description { get; init; }

    /// <summary>Amount in the site's base currency.</summary>
    public required decimal Amount { get; init; }
}

/// <summary>A single FinancialRecord read-model record, including its line items.</summary>
public sealed record FinancialRecordDetailDto
{
    /// <summary>Financial record identifier.</summary>
    public required Guid FinancialRecordId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Current lifecycle status (Draft/Submitted/Approved/Rejected/Closed/Archived).</summary>
    public required string Status { get; init; }

    /// <summary>Financial category (e.g. Budget, Cost, Forecast).</summary>
    public required string Category { get; init; }

    /// <summary>Monetary amount.</summary>
    public required decimal Amount { get; init; }

    /// <summary>Line items recorded against this financial record.</summary>
    public IReadOnlyList<FinancialRecordLineItemDto> LineItems { get; init; } = Array.Empty<FinancialRecordLineItemDto>();

    /// <summary>UTC timestamp the record was recorded.</summary>
    public required DateTime RecordedAtUtc { get; init; }

    /// <summary>Identity of the user who recorded this financial record.</summary>
    public required Guid RecordedBy { get; init; }

    /// <summary>
    /// Optimistic-concurrency token (native RowVersion projected as a hex string). Callers must
    /// echo this value back on a subsequent mutating command's ETag field; a stale/mismatched
    /// value results in a concurrency-conflict error.
    /// </summary>
    public string? ETag { get; init; }
}

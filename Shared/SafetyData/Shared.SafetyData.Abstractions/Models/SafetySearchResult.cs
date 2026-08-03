// ============================================================================
//  Project:        Shared.SafetyData
//  Module:         Abstractions.Models
//  Layer:          Abstractions
//  File:           SafetySearchResult.cs
//  Created:        2026-07-27
//  Author:         Platform Team
//  SchemaVersion:  2026.1
//  Description:    Generic paginated result envelope, mirroring
//                  Shared.AuditLogging's AuditLogSearchResult shape. Generic
//                  over the entry type (rather than one non-generic result type
//                  per entry, e.g. HazardSummarySearchResult /
//                  CorrectiveActionSearchResult) because Hazard Summary and
//                  Corrective Action both page identically (count + page of
//                  rows via a stored procedure with @PageNumber/@PageSize) and
//                  a single generic type avoids duplicating that shape twice.
//                  Safety Compliance is a small, non-paginated rollup (see
//                  SafetyComplianceEntry) and does not use this envelope --
//                  ISafetyDataRepository.GetSafetyComplianceAsync returns
//                  IReadOnlyList&lt;SafetyComplianceEntry&gt; directly.
//                  Additive-only evolution.
// ============================================================================
namespace Shared.SafetyData.Abstractions.Models;

/// <summary>A single page of Shared.SafetyData search results plus paging metadata.</summary>
/// <typeparam name="TEntry">The entry type for this page -- <see cref="HazardSummaryEntry"/> or <see cref="CorrectiveActionEntry"/>.</typeparam>
/// <param name="Items">The entries for the requested page.</param>
/// <param name="PageNumber">The 1-based page number returned.</param>
/// <param name="PageSize">The page size requested.</param>
/// <param name="TotalCount">The total number of rows matching the filter, across all pages.</param>
public sealed record SafetySearchResult<TEntry>(
    IReadOnlyList<TEntry> Items,
    int PageNumber,
    int PageSize,
    int TotalCount);

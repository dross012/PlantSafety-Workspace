// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Shared query-string filter for all Checklists sub-component analytics endpoints.

namespace PlantSafety.Api.Checklists.Contracts.Requests;

/// <summary>Query-string filters shared by every Checklists analytics sub-component endpoint.</summary>
/// <param name="SiteId">Optional site filter. Null returns data for all accessible sites.</param>
/// <param name="From">Inclusive start of the date range (UTC).</param>
/// <param name="To">Inclusive end of the date range (UTC).</param>
public sealed record ChecklistsAnalyticsQueryRequest(
    Guid? SiteId,
    DateTime From,
    DateTime To);

// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Shared query-string filter for all SafetyCulture sub-component analytics endpoints.

namespace PlantSafety.Api.SafetyCulture.Contracts.Requests;

/// <summary>Query-string filters shared by every SafetyCulture analytics sub-component endpoint.</summary>
/// <param name="SiteId">Optional site filter. Null returns data for all accessible sites.</param>
/// <param name="From">Inclusive start of the date range (UTC).</param>
/// <param name="To">Inclusive end of the date range (UTC).</param>
public sealed record SafetyCultureAnalyticsQueryRequest(
    Guid? SiteId,
    DateTime From,
    DateTime To);

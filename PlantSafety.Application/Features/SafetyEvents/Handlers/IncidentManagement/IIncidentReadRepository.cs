// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-side repository interface for Incident query operations.
//   Implemented in PlantSafety.Infrastructure (read-optimised stored procedures, Dapper).
// SchemaVersion: 1

using PlantSafety.Application.Features.SafetyEvents.Mappings.IncidentManagement;

namespace PlantSafety.Application.Features.SafetyEvents.Handlers.IncidentManagement;

/// <summary>
/// Read-side repository for Incident query operations.
/// Returns Infrastructure read models; mapping to DTOs is performed by the handler.
/// </summary>
public interface IIncidentReadRepository
{
    /// <summary>
    /// Returns the <see cref="IncidentReadModel"/> for the given incident, or <c>null</c> if not found.
    /// Scoped to the caller's <paramref name="tenantId"/>.
    /// </summary>
    Task<IncidentReadModel?> GetByIdAsync(
        Guid tenantId,
        Guid incidentId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Returns a page of <see cref="IncidentReadModel"/> records matching the given filters.
    /// The returned tuple contains the page items and the total count (for pagination metadata).
    /// </summary>
    Task<(IReadOnlyList<IncidentReadModel> Items, int TotalCount)> ListAsync(
        Guid tenantId,
        Guid? siteId,
        string? status,
        string? severity,
        DateTimeOffset? occurredFromUtc,
        DateTimeOffset? occurredToUtc,
        string orderBy,
        string sortDirection,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);

    /// <summary>
    /// Full-text searches Incidents by <paramref name="searchTerm"/> across title, description,
    /// and location. The returned tuple contains the page items and the total match count.
    /// </summary>
    Task<(IReadOnlyList<IncidentReadModel> Items, int TotalCount)> SearchAsync(
        Guid tenantId,
        string searchTerm,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);

    /// <summary>
    /// Returns up to <paramref name="maxRecords"/> Incident records matching the given filters,
    /// for bulk export. Unpaged — the caller is responsible for serialising the result.
    /// </summary>
    Task<IReadOnlyList<IncidentReadModel>> ExportAsync(
        Guid tenantId,
        Guid? siteId,
        DateTimeOffset? occurredFromUtc,
        DateTimeOffset? occurredToUtc,
        int maxRecords,
        CancellationToken cancellationToken);
}

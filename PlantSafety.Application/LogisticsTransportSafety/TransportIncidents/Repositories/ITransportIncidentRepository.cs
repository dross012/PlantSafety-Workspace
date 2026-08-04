// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Repository abstraction for the TransportIncident read-model.
// SchemaVersion: 1

using PlantSafety.Application.LogisticsTransportSafety.TransportIncidents.Models;

namespace PlantSafety.Application.LogisticsTransportSafety.TransportIncidents.Repositories;

/// <summary>Read-only repository for TransportIncident data. Implemented in the Infrastructure layer.</summary>
public interface ITransportIncidentRepository
{
    /// <summary>Returns TransportIncident items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<TransportIncidentItemDto>> GetTransportIncidentHistoryAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}

// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Repository abstraction for the CommunicationLogEntry read-model.
// SchemaVersion: 1

using PlantSafety.Application.PublicSafetyCommunication.CommunicationLog.Models;

namespace PlantSafety.Application.PublicSafetyCommunication.CommunicationLog.Repositories;

/// <summary>Read-only repository for CommunicationLogEntry data. Implemented in the Infrastructure layer.</summary>
public interface ICommunicationLogEntryRepository
{
    /// <summary>Returns CommunicationLogEntry items for the tenant/incident, ordered chronologically.</summary>
    Task<IReadOnlyList<CommunicationLogEntryItemDto>> GetCommunicationLogByIncidentAsync(
        Guid tenantId,
        Guid incidentId,
        CancellationToken cancellationToken);
}

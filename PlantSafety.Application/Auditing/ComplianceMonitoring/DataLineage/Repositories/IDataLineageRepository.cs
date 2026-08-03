// AUTHOR: PlantSafety Platform Team (Claude Code performance-optimization-agent)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Repository abstraction for cross-system data-lineage reconstruction.
//                    Implemented in the Infrastructure layer by composing the existing Outbox
//                    and DeadLetterEvents read paths -- not a new heavyweight lineage engine.
// SchemaVersion: 1

using PlantSafety.Application.Auditing.ComplianceMonitoring.DataLineage.Models;

namespace PlantSafety.Application.Auditing.ComplianceMonitoring.DataLineage.Repositories;

/// <summary>Read-only repository reconstructing the event chain for a given correlation identifier.</summary>
public interface IDataLineageRepository
{
    /// <summary>
    /// Returns every outbox-published and dead-lettered event sharing <paramref name="correlationId"/>
    /// for the given tenant, oldest first.
    /// </summary>
    Task<IReadOnlyList<DataLineageEntryDto>> GetLineageAsync(
        Guid tenantId, Guid correlationId, CancellationToken cancellationToken);
}

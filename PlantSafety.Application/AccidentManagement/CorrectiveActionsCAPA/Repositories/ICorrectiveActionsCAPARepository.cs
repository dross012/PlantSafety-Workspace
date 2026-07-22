// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the Corrective Actions (CAPA) read-model.
// SchemaVersion: 1

using PlantSafety.Application.AccidentManagement.CorrectiveActionsCAPA.Models;

namespace PlantSafety.Application.AccidentManagement.CorrectiveActionsCAPA.Repositories;

/// <summary>Read-only repository for Corrective Actions (CAPA) data. Implemented in the Infrastructure layer.</summary>
public interface ICorrectiveActionsCAPARepository
{
    /// <summary>Returns Corrective Actions (CAPA) items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<CorrectiveActionsCAPAItemDto>> GetCorrectiveActionsCAPAAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
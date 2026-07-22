// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the Process Safety Information (PSI) read-model.
// SchemaVersion: 1

using PlantSafety.Application.RiskManagement.ProcessSafetyInformation.Models;

namespace PlantSafety.Application.RiskManagement.ProcessSafetyInformation.Repositories;

/// <summary>Read-only repository for Process Safety Information (PSI) data. Implemented in the Infrastructure layer.</summary>
public interface IProcessSafetyInformationRepository
{
    /// <summary>Returns Process Safety Information (PSI) items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<ProcessSafetyInformationItemDto>> GetProcessSafetyInformationAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
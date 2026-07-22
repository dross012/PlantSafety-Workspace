// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the Leading Indicators (Training, Audit, CAPA closure, PM compliance) read-model.
// SchemaVersion: 1

using PlantSafety.Application.Scorecards.LeadingIndicators.Models;

namespace PlantSafety.Application.Scorecards.LeadingIndicators.Repositories;

/// <summary>Read-only repository for Leading Indicators (Training, Audit, CAPA closure, PM compliance) data. Implemented in the Infrastructure layer.</summary>
public interface ILeadingIndicatorsRepository
{
    /// <summary>Returns Leading Indicators (Training, Audit, CAPA closure, PM compliance) items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<LeadingIndicatorsItemDto>> GetLeadingIndicatorsAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
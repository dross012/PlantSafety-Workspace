// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the Site Securing, Injury Triage & Emergency Plan Execution read-model.
// SchemaVersion: 1

using PlantSafety.Application.AccidentManagement.SiteSecuringAndTriage.Models;

namespace PlantSafety.Application.AccidentManagement.SiteSecuringAndTriage.Repositories;

/// <summary>Read-only repository for Site Securing, Injury Triage &amp; Emergency Plan Execution data. Implemented in the Infrastructure layer.</summary>
public interface ISiteSecuringAndTriageRepository
{
    /// <summary>Returns Site Securing, Injury Triage &amp; Emergency Plan Execution items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<SiteSecuringAndTriageItemDto>> GetSiteSecuringAndTriageAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
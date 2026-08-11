// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the Safety Training & Reporting Policies read-model.
// SchemaVersion: 1

using PlantSafety.Application.SafetyPolicies.SafetyTrainingReporting.Models;

namespace PlantSafety.Application.SafetyPolicies.SafetyTrainingReporting.Repositories;

/// <summary>Read-only repository for Safety Training &amp; Reporting Policies data. Implemented in the Infrastructure layer.</summary>
public interface ISafetyTrainingReportingRepository
{
    /// <summary>Returns Safety Training &amp; Reporting Policies items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<SafetyTrainingReportingItemDto>> GetSafetyTrainingReportingAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
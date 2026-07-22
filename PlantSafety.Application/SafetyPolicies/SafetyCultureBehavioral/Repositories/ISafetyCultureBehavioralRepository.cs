// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the Safety Culture & Behavioral Policies read-model.
// SchemaVersion: 1

using PlantSafety.Application.SafetyPolicies.SafetyCultureBehavioral.Models;

namespace PlantSafety.Application.SafetyPolicies.SafetyCultureBehavioral.Repositories;

/// <summary>Read-only repository for Safety Culture & Behavioral Policies data. Implemented in the Infrastructure layer.</summary>
public interface ISafetyCultureBehavioralRepository
{
    /// <summary>Returns Safety Culture & Behavioral Policies items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<SafetyCultureBehavioralItemDto>> GetSafetyCultureBehavioralAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
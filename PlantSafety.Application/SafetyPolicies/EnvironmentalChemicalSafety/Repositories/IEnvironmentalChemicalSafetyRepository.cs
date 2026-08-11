// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the Environmental & Chemical Safety Policies read-model.
// SchemaVersion: 1

using PlantSafety.Application.SafetyPolicies.EnvironmentalChemicalSafety.Models;

namespace PlantSafety.Application.SafetyPolicies.EnvironmentalChemicalSafety.Repositories;

/// <summary>Read-only repository for Environmental &amp; Chemical Safety Policies data. Implemented in the Infrastructure layer.</summary>
public interface IEnvironmentalChemicalSafetyRepository
{
    /// <summary>Returns Environmental &amp; Chemical Safety Policies items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<EnvironmentalChemicalSafetyItemDto>> GetEnvironmentalChemicalSafetyAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
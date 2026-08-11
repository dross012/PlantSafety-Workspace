// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Repository abstraction for the Security & Access Control Policies read-model.
// SchemaVersion: 1

using PlantSafety.Application.SafetyPolicies.SecurityAccessControl.Models;

namespace PlantSafety.Application.SafetyPolicies.SecurityAccessControl.Repositories;

/// <summary>Read-only repository for Security &amp; Access Control Policies data. Implemented in the Infrastructure layer.</summary>
public interface ISecurityAccessControlRepository
{
    /// <summary>Returns Security &amp; Access Control Policies items for the tenant/site within the given UTC date range.</summary>
    Task<IReadOnlyList<SecurityAccessControlItemDto>> GetSecurityAccessControlAsync(
        Guid tenantId,
        Guid? siteId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken);
}
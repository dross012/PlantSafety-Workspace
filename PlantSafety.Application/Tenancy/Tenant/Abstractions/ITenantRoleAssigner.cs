// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Application-layer abstraction for the "assign roles" step of the automated
//   tenant provisioning pipeline (Phase 7 §2.1). Assigns the Phase 6 Shared.Authorization
//   EnterpriseRole tier (via RoleInheritanceResolver's canonical role model) to the newly created
//   tenant admin user.
// SchemaVersion: 1

using Shared.Authorization.Abstractions.Roles;

namespace PlantSafety.Application.Tenancy.Tenant.Abstractions;

/// <summary>Assigns the initial <see cref="EnterpriseRole"/> tier to a newly provisioned tenant's admin user.</summary>
public interface ITenantRoleAssigner
{
    /// <summary>
    /// Assigns <paramref name="role"/> (and, per <see cref="RoleInheritanceResolver"/>, every role
    /// it inherits) to <paramref name="userId"/> within <paramref name="tenantId"/>.
    /// </summary>
    /// <param name="tenantId">The tenant the role assignment is scoped to.</param>
    /// <param name="userId">The user receiving the role.</param>
    /// <param name="role">The highest-ranked role to assign.</param>
    /// <param name="correlationId">Correlation identifier propagated from the provisioning command.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task AssignRoleAsync(Guid tenantId, Guid userId, EnterpriseRole role, Guid correlationId, CancellationToken cancellationToken);
}

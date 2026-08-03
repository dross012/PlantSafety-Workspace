// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Application-layer abstraction for the "create admin user" step of the
//   automated tenant provisioning pipeline (Phase 7 §2.1). Implemented in Infrastructure by
//   dispatching the existing EnterpriseFabric.Users CreateUserCommand (Phase 6 CrossDomain.Users/
//   Azure.Identity.Application surface) with the platform-admin-tier role -- reusing the
//   established user-creation pipeline rather than duplicating it.
// SchemaVersion: 1

namespace PlantSafety.Application.Tenancy.Tenant.Abstractions;

/// <summary>Provisions the initial administrative user for a newly provisioned tenant.</summary>
public interface ITenantAdminUserProvisioner
{
    /// <summary>
    /// Creates the tenant's initial administrative user account, returning its new UserId.
    /// </summary>
    /// <param name="tenantId">The newly provisioned tenant's identity.</param>
    /// <param name="adminEmail">Email address for the initial admin account.</param>
    /// <param name="adminFirstName">First name for the initial admin account.</param>
    /// <param name="adminLastName">Last name for the initial admin account.</param>
    /// <param name="correlationId">Correlation identifier propagated from the provisioning command.</param>
    /// <param name="causationId">Causation identifier propagated from the provisioning command.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task<Guid> ProvisionAdminUserAsync(
        Guid tenantId, string adminEmail, string adminFirstName, string adminLastName,
        Guid correlationId, Guid causationId, CancellationToken cancellationToken);
}

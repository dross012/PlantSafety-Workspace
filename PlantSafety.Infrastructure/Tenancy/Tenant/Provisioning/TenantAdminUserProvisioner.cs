// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tenancy.Tenant
//  Layer:          Infrastructure
//  File:           TenantAdminUserProvisioner.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §2.1 Automated Tenant Creation)
//  SchemaVersion:  2026.1
//  Description:    ITenantAdminUserProvisioner implementation that reuses the existing Phase 6
//                  EnterpriseFabric.Users CreateUserCommand pipeline (the platform's real,
//                  established user-creation surface -- CrossDomain.Users only exposes read
//                  queries; EnterpriseFabric.Users.Commands.Create.CreateUserCommand is the real
//                  write path) rather than duplicating user-creation logic. The initial admin
//                  user is created with the PlatformAdmin role pre-assigned via CreateUserCommand's
//                  own Roles list (its established mechanism for granting roles at creation time);
//                  ITenantRoleAssigner performs an additional, explicit Tenancy-scoped record of
//                  that role grant for Tenancy's own audit trail (see its own file header).
//
//                  IMPORTANT: this injects IRequestHandler&lt;CreateUserCommand, Guid&gt; DIRECTLY
//                  rather than dispatching through ISender.Send. CreateUserCommand.TenantId is the
//                  BRAND-NEW tenant's id, which never equals the platform-admin caller's own
//                  resolved IOperationContext.TenantId (their platform-operator tenant) -- routing
//                  through ISender.Send would run the full MediatR pipeline, including
//                  TenantIsolationBehavior, which rejects any nested command whose TenantId
//                  mismatches the ambient operation context, exactly the false-positive this
//                  cross-tenant provisioning step would otherwise trip. Invoking the handler
//                  directly is the standard technique for legitimate, system-level cross-tenant
//                  dispatch and is a deliberate, narrow bypass of the pipeline's per-request
//                  cross-cutting behaviors (validation/logging/authorization) for this one
//                  bootstrap call -- NOT a general precedent for skipping TenantIsolationBehavior.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using PlantSafety.Application.Features.EnterpriseFabric.Commands.Users.Create;
using PlantSafety.Application.Tenancy.Tenant.Abstractions;
using Shared.Authorization.Abstractions.Roles;

namespace PlantSafety.Infrastructure.Tenancy.Tenant.Provisioning;

/// <summary>Provisions a tenant's initial administrative user by invoking the existing EnterpriseFabric.Users CreateUserCommand handler directly.</summary>
public sealed class TenantAdminUserProvisioner(IRequestHandler<CreateUserCommand, Guid> createUserHandler) : ITenantAdminUserProvisioner
{
    /// <inheritdoc/>
    public async Task<Guid> ProvisionAdminUserAsync(
        Guid tenantId, string adminEmail, string adminFirstName, string adminLastName,
        Guid correlationId, Guid causationId, CancellationToken cancellationToken)
    {
        var command = new CreateUserCommand
        {
            CommandId = Guid.NewGuid(),
            CorrelationId = correlationId,
            CausationId = causationId,
            RequestedAtUtc = DateTime.UtcNow,
            ApiVersionToken = "v1",
            TenantId = tenantId,
            ActorId = tenantId, // Self-provisioning system action: the newly provisioned tenant is its own actor of record for this bootstrap step.
            FirstName = adminFirstName,
            LastName = adminLastName,
            Email = adminEmail,
            Roles = [EnterpriseRole.PlatformAdmin.Name],
            SiteIds = [],
        };

        return await createUserHandler.Handle(command, cancellationToken);
    }
}

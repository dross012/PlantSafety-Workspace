// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Handler for ProvisionTenantCommand -- the real, end-to-end automated tenant
//   provisioning pipeline (Phase 7 §2.1):
//     1. Validate the request against TenantProvisioningPolicy and name uniqueness.
//     2. Create the Tenant aggregate (Provisioning status) and persist it.
//     3. RLS/session-context initialization: no separate step is required here -- every
//        subsequent write in this pipeline (and every future tenant-scoped write) already
//        initializes SQL Server SESSION_CONTEXT('TenantId') via
//        TenantSessionContextExtensions.SetTenantSessionContextAsync as part of the existing
//        Dapper connection-opening path, keyed by this tenant's own newly minted TenantId --
//        there is nothing further to bootstrap.
//     4. Seed default reference data: the Tenant aggregate is created with
//        TenantConfiguration.CreateDefault() (no optional modules enabled, no feature-flag
//        overrides, no custom compliance rules -- the safest, most restrictive starting point).
//     5. Create the tenant's initial admin user (ITenantAdminUserProvisioner).
//     6. Assign roles (ITenantRoleAssigner, backed by RoleInheritanceResolver).
//     7. Generate the tenant's primary API key (ITenantApiKeyGenerator).
//     8. Activate the tenant (Provisioning -> Active) and persist.
//   Each late-pipeline step (5-7) that fails after the Tenant row is already persisted leaves the
//   tenant in Provisioning (not Active) rather than partially Active, so a platform admin can see
//   the pipeline did not complete and retry/inspect rather than the tenant silently operating with
//   a missing admin user or API key.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Authorization.Abstractions.Roles;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Tenancy.Tenant.Interfaces;
using PlantSafety.Domain.Tenancy.Tenant.Policies;
using PlantSafety.Domain.Tenancy.Tenant.ValueObjects;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Tenancy.Tenant.Abstractions;
using PlantSafety.Application.Tenancy.Tenant.Mapping;

namespace PlantSafety.Application.Tenancy.Tenant.Commands;

/// <summary>Handles <see cref="ProvisionTenantCommand"/>: the full automated tenant-provisioning pipeline.</summary>
public sealed class ProvisionTenantCommandHandler : IRequestHandler<ProvisionTenantCommand, Result<TenantProvisioningResultDto>>
{
    private readonly ITenantFactory _factory;
    private readonly ITenantRepository _repository;
    private readonly ITenantAdminUserProvisioner _adminUserProvisioner;
    private readonly ITenantRoleAssigner _roleAssigner;
    private readonly ITenantApiKeyGenerator _apiKeyGenerator;
    private readonly ILogger<ProvisionTenantCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ProvisionTenantCommandHandler(
        ITenantFactory factory,
        ITenantRepository repository,
        ITenantAdminUserProvisioner adminUserProvisioner,
        ITenantRoleAssigner roleAssigner,
        ITenantApiKeyGenerator apiKeyGenerator,
        ILogger<ProvisionTenantCommandHandler> logger)
    {
        _factory = factory;
        _repository = repository;
        _adminUserProvisioner = adminUserProvisioner;
        _roleAssigner = roleAssigner;
        _apiKeyGenerator = apiKeyGenerator;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<TenantProvisioningResultDto>> Handle(ProvisionTenantCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!TenantProvisioningPolicy.IsWellFormedName(request.Name))
            {
                return Result<TenantProvisioningResultDto>.Failure(new ApplicationError(
                    "Tenancy.Tenant.InvalidName", "Tenant name is required and must not exceed the maximum length.", Guid.NewGuid()));
            }

            if (!TenantProvisioningPolicy.IsSupportedRegion(request.Region))
            {
                return Result<TenantProvisioningResultDto>.Failure(new ApplicationError(
                    "Tenancy.Tenant.UnsupportedRegion", $"'{request.Region}' is not a supported deployment region.", Guid.NewGuid()));
            }

            if (!SubscriptionPlanCode.TryParse(request.PlanCode, out var planCode))
            {
                return Result<TenantProvisioningResultDto>.Failure(new ApplicationError(
                    "Tenancy.Tenant.UnknownPlanCode", $"'{request.PlanCode}' is not a recognized subscription plan code.", Guid.NewGuid()));
            }

            if (await _repository.NameExistsAsync(request.Name, cancellationToken))
            {
                return Result<TenantProvisioningResultDto>.Failure(new ApplicationError(
                    "Tenancy.Tenant.NameAlreadyExists", $"A tenant named '{request.Name}' already exists.", Guid.NewGuid()));
            }

            var actor = PerformedByActor.User(UserId.FromGuid(request.ActorId));
            var correlationId = new CorrelationId(request.CorrelationId);
            var causationId = new CausationId(request.CausationId);
            var nowUtc = UtcTimestamp.Now();

            var tenant = _factory.Provision(request.Name, request.Region, planCode, nowUtc, actor, correlationId, causationId);
            await _repository.AddAsync(tenant, cancellationToken);

            _logger.LogInformation(
                "TenantProvisioning: Tenant {TenantId} ('{Name}', region {Region}) record created -- beginning admin user/role/API key steps.",
                tenant.TenantId, tenant.Name, tenant.Region);

            var adminUserId = await _adminUserProvisioner.ProvisionAdminUserAsync(
                tenant.TenantId.Value, request.AdminEmail, request.AdminFirstName, request.AdminLastName,
                request.CorrelationId, request.CausationId, cancellationToken);

            await _roleAssigner.AssignRoleAsync(
                tenant.TenantId.Value, adminUserId, EnterpriseRole.PlatformAdmin, request.CorrelationId, cancellationToken);

            var apiKey = await _apiKeyGenerator.GenerateAsync(tenant.TenantId.Value, cancellationToken);

            tenant.Activate(UtcTimestamp.Now(), actor, correlationId, causationId);
            await _repository.UpdateAsync(tenant, cancellationToken);

            _logger.LogInformation(
                "TenantProvisioning: Tenant {TenantId} ('{Name}') fully provisioned and Active. AdminUserId={AdminUserId}",
                tenant.TenantId, tenant.Name, adminUserId);

            return Result<TenantProvisioningResultDto>.Success(
                new TenantProvisioningResultDto(TenantMapper.ToDto(tenant), adminUserId, apiKey));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TenantProvisioning: failed to provision tenant '{Name}' in region '{Region}'.", request.Name, request.Region);

            return Result<TenantProvisioningResultDto>.Failure(new ApplicationError(
                "Tenancy.Tenant.ProvisioningFailed", "Unable to provision the tenant at this time.", Guid.NewGuid()));
        }
    }
}

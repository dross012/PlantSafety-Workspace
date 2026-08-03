// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Handler for SetTenantFeatureFlagOverrideCommand. Resolves the
//   caller's identity via ICurrentUserService (never SYSTEM_USER).
// SchemaVersion: 1

using System;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.CurrentUser;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operations.Configuration.Commands;
using PlantSafety.Application.Operations.Configuration.Models;
using PlantSafety.Application.Operations.Configuration.Repositories;
using PlantSafety.Application.Tenancy.Tenant.Repositories;
using PlantSafety.Domain.Billing.Subscription.Catalog;
using PlantSafety.Domain.Tenancy.Tenant.ValueObjects;

namespace PlantSafety.Application.Operations.Configuration.Handlers;

/// <summary>Handles <see cref="SetTenantFeatureFlagOverrideCommand"/>.</summary>
public sealed class SetTenantFeatureFlagOverrideCommandHandler : IRequestHandler<SetTenantFeatureFlagOverrideCommand, Result<TenantFeatureFlagOverrideDto>>
{
    private const string SystemIdentity = "System";

    private readonly ITenantFeatureFlagOverrideRepository _repository;
    private readonly ITenantReadRepository _tenantReadRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<SetTenantFeatureFlagOverrideCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public SetTenantFeatureFlagOverrideCommandHandler(
        ITenantFeatureFlagOverrideRepository repository,
        ITenantReadRepository tenantReadRepository,
        ICurrentUserService currentUserService,
        ILogger<SetTenantFeatureFlagOverrideCommandHandler> logger)
    {
        _repository = repository;
        _tenantReadRepository = tenantReadRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<TenantFeatureFlagOverrideDto>> Handle(SetTenantFeatureFlagOverrideCommand request, CancellationToken cancellationToken)
    {
        var updatedBy = _currentUserService.IsAuthenticated
            ? _currentUserService.UserId.ToString()
            : SystemIdentity;

        // Phase 7 §3.4 License Enforcement: a tenant's SubscriptionPlan determines which modules'
        // feature flags CAN be enabled, not just whether they currently ARE. Only enforced when
        // ENABLING an override -- disabling a module is always permitted regardless of plan.
        if (request.Enabled)
        {
            var tenant = await _tenantReadRepository.GetByIdAsync(request.OverrideTenantId, cancellationToken);
            if (tenant is not null)
            {
                var plan = SubscriptionPlanCatalog.Resolve(SubscriptionPlanCode.From(tenant.PlanCode));
                if (!plan.Limits.IsModuleAllowed(request.FeatureName))
                {
                    _logger.LogWarning(
                        "Tenant feature flag override blocked for {FeatureName}/tenant {OverrideTenantId}: not permitted under plan '{PlanCode}'.",
                        request.FeatureName, request.OverrideTenantId, plan.Tier.Code);

                    return Result<TenantFeatureFlagOverrideDto>.Failure(new ApplicationError(
                        "Operations.Configuration.ModuleNotAllowedByPlan",
                        $"Module/feature '{request.FeatureName}' is not included in tenant's '{plan.Tier.Code}' subscription plan.",
                        Guid.NewGuid()));
                }
            }
        }

        try
        {
            var result = await _repository.SetAsync(
                request.OverrideTenantId, request.FeatureName, request.Enabled, request.RolloutPercentage, updatedBy, cancellationToken);

            _logger.LogInformation(
                "Tenant feature flag override for {FeatureName}/tenant {OverrideTenantId} set to Enabled={Enabled}, RolloutPercentage={RolloutPercentage} by {UpdatedBy}.",
                request.FeatureName, request.OverrideTenantId, request.Enabled, request.RolloutPercentage, updatedBy);

            return Result<TenantFeatureFlagOverrideDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to set tenant feature flag override for {FeatureName}/tenant {OverrideTenantId} (requested by {UpdatedBy}).",
                request.FeatureName, request.OverrideTenantId, updatedBy);

            return Result<TenantFeatureFlagOverrideDto>.Failure(new ApplicationError(
                "Operations.Configuration.TenantOverrideFailed",
                "Unable to update the tenant feature flag override at this time.",
                Guid.NewGuid()));
        }
    }
}

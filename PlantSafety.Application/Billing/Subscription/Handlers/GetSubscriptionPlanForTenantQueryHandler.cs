// AUTHOR: PlantSafety Platform Team (Phase 7 §3.2 Subscription Plans)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Handler for GetSubscriptionPlanForTenantQuery.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Billing.Subscription.Mapping;
using PlantSafety.Application.Billing.Subscription.Models;
using PlantSafety.Application.Billing.Subscription.Queries;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Tenancy.Tenant.Repositories;
using PlantSafety.Domain.Billing.Subscription.Catalog;
using PlantSafety.Domain.Tenancy.Tenant.ValueObjects;

namespace PlantSafety.Application.Billing.Subscription.Handlers;

/// <summary>Handles <see cref="GetSubscriptionPlanForTenantQuery"/>.</summary>
public sealed class GetSubscriptionPlanForTenantQueryHandler(ITenantReadRepository tenantReadRepository)
    : IRequestHandler<GetSubscriptionPlanForTenantQuery, Result<SubscriptionPlanDto>>
{
    /// <inheritdoc/>
    public async Task<Result<SubscriptionPlanDto>> Handle(GetSubscriptionPlanForTenantQuery request, CancellationToken cancellationToken)
    {
        var tenant = await tenantReadRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
        {
            return Result<SubscriptionPlanDto>.Failure(new ApplicationError(
                "Billing.Subscription.TenantNotFound", $"No tenant found with id '{request.TenantId}'.", Guid.NewGuid()));
        }

        var planCode = SubscriptionPlanCode.From(tenant.PlanCode);
        var plan = SubscriptionPlanCatalog.Resolve(planCode);

        return Result<SubscriptionPlanDto>.Success(SubscriptionPlanMapper.ToDto(plan));
    }
}

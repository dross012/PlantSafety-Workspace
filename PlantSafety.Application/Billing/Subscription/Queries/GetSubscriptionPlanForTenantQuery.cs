// AUTHOR: PlantSafety Platform Team (Phase 7 §3.2 Subscription Plans)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Query resolving a tenant's active SubscriptionPlan catalog entry via
//   Tenancy.Tenant's existing PlanCode -- the additive bridge described on
//   SubscriptionPlanCatalog.Resolve(SubscriptionPlanCode).
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Billing.Subscription.Models;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Billing.Subscription.Queries;

/// <summary>Retrieves the SubscriptionPlan catalog entry currently active for the given tenant.</summary>
/// <param name="TenantId">Tenant identifier to resolve the active plan for.</param>
public sealed record GetSubscriptionPlanForTenantQuery(Guid TenantId) : IRequest<Result<SubscriptionPlanDto>>;

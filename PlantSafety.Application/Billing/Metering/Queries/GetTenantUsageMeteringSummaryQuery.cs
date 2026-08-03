// AUTHOR: PlantSafety Platform Team (Phase 7 §3.1 Usage Metering)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Query composing every existing platform data source needed to produce a
//   tenant's usage-metering summary for a single billing period (Handbook Phase 5/6
//   cross-module read-model composition convention).
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Billing.Metering.Models;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Billing.Metering.Queries;

/// <summary>Composes a tenant's usage across every metered dimension for the given calendar-month billing period (UTC).</summary>
/// <param name="TenantId">Tenant to compose usage metering for.</param>
/// <param name="PeriodYear">Calendar year of the billing period (UTC).</param>
/// <param name="PeriodMonth">Calendar month of the billing period (UTC), 1-12.</param>
public sealed record GetTenantUsageMeteringSummaryQuery(Guid TenantId, int PeriodYear, int PeriodMonth)
    : IRequest<Result<TenantUsageMeteringSummaryDto>>;

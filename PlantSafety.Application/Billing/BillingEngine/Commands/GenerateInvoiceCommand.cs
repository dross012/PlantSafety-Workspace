// AUTHOR: PlantSafety Platform Team (Phase 7 §3.3 Billing Engine)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Command generating a Draft Invoice for a tenant's billing period from an
//   already-captured UsageMeteringSnapshot (see CaptureUsageMeteringSnapshotCommand) and the
//   tenant's active SubscriptionPlan.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Billing.BillingEngine.Models;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Billing.BillingEngine.Commands;

/// <summary>A discount or credit to apply to a generated invoice, as raw request data.</summary>
/// <param name="Kind">One of <c>"PercentageDiscount"</c>, <c>"FixedAmountDiscount"</c>, or <c>"Credit"</c>.</param>
/// <param name="Description">Human-readable description shown on the invoice.</param>
/// <param name="PercentageValue">Required (0-100) when <paramref name="Kind"/> is <c>"PercentageDiscount"</c>.</param>
/// <param name="FixedAmount">Required (positive) when <paramref name="Kind"/> is <c>"FixedAmountDiscount"</c> or <c>"Credit"</c>.</param>
public sealed record DiscountOrCreditRequestDto(string Kind, string Description, decimal? PercentageValue, decimal? FixedAmount);

/// <summary>Generates a Draft invoice for the given tenant/billing period.</summary>
/// <param name="TenantId">Tenant to generate the invoice for.</param>
/// <param name="PeriodYear">Calendar year of the billing period (UTC).</param>
/// <param name="PeriodMonth">Calendar month of the billing period (UTC), 1-12.</param>
/// <param name="DiscountsAndCredits">Discounts/credits to apply, in application order. May be empty.</param>
public sealed record GenerateInvoiceCommand(
    Guid TenantId, int PeriodYear, int PeriodMonth, IReadOnlyList<DiscountOrCreditRequestDto> DiscountsAndCredits)
    : IRequest<Result<InvoiceDto>>;

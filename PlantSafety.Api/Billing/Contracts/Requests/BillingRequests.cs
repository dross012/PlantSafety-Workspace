// ============================================================================
// File: BillingRequests.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team (Phase 7 §6.1 Admin Portal / Billing)
// Created: 2026-08-03
// Description: External-facing request DTOs for the Billing module's V1 controller. Mirrors
//              the Application layer's GenerateInvoiceCommand/RefundInvoiceCommand shapes for
//              the HTTP boundary only -- no business logic here.
// ============================================================================

namespace PlantSafety.Api.Billing.Contracts.Requests;

/// <summary>A discount or credit to apply to a generated invoice, as an HTTP request field.</summary>
public sealed record DiscountOrCreditRequest
{
    /// <summary>One of <c>"PercentageDiscount"</c>, <c>"FixedAmountDiscount"</c>, or <c>"Credit"</c>.</summary>
    public string Kind { get; init; } = string.Empty;

    /// <summary>Human-readable description shown on the invoice.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Required (0-100) when <see cref="Kind"/> is <c>"PercentageDiscount"</c>.</summary>
    public decimal? PercentageValue { get; init; }

    /// <summary>Required (positive) when <see cref="Kind"/> is <c>"FixedAmountDiscount"</c> or <c>"Credit"</c>.</summary>
    public decimal? FixedAmount { get; init; }
}

/// <summary>Request body for generating a draft invoice for a tenant's billing period.</summary>
public sealed record GenerateInvoiceRequest
{
    /// <summary>Calendar year of the billing period (UTC).</summary>
    public int PeriodYear { get; init; }

    /// <summary>Calendar month of the billing period (UTC), 1-12.</summary>
    public int PeriodMonth { get; init; }

    /// <summary>Discounts/credits to apply, in application order. May be empty.</summary>
    public IReadOnlyList<DiscountOrCreditRequest> DiscountsAndCredits { get; init; } = [];
}

/// <summary>Request body for refunding a previously paid invoice.</summary>
public sealed record RefundInvoiceRequest
{
    /// <summary>Reason recorded for the refund/void.</summary>
    public string Reason { get; init; } = string.Empty;
}

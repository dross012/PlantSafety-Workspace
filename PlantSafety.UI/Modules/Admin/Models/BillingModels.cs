// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-08-03
// FILE DESCRIPTION: Client-side DTOs for the Admin Portal's Billing tab (Phase 7 §6.1 Admin
//   Portal / §3 Billing). Mirrors PlantSafety.Application.Billing.* read models and the new
//   BillingV1Controller request contracts.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.Admin.Models;

/// <summary>Canonical read model for a SubscriptionPlan catalog entry (mirrors <c>SubscriptionPlanDto</c>).</summary>
public sealed class SubscriptionPlanDto
{
    /// <summary>Gets the stable machine-readable plan tier code.</summary>
    public string PlanTierCode { get; init; } = string.Empty;

    /// <summary>Gets the human-readable plan name.</summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>Gets the base recurring monthly price amount.</summary>
    public decimal MonthlyBasePriceAmount { get; init; }

    /// <summary>Gets the ISO-4217 currency code for all monetary fields.</summary>
    public string Currency { get; init; } = string.Empty;

    /// <summary>Gets the maximum active user accounts permitted, or null if unlimited.</summary>
    public int? MaxUsers { get; init; }

    /// <summary>Gets the maximum API requests per minute before throttling.</summary>
    public int ApiRateLimitPerMinute { get; init; }

    /// <summary>Gets the maximum blob storage in bytes, or null if unlimited.</summary>
    public long? StorageQuotaBytes { get; init; }

    /// <summary>Gets the closed set of module identifiers this plan permits, or null if every module is permitted.</summary>
    public IReadOnlyList<string>? AllowedModuleCodes { get; init; }

    /// <summary>Gets the maximum scheduled task runs per billing period, or null if unlimited.</summary>
    public int? MaxScheduledTasksPerMonth { get; init; }

    /// <summary>Gets the maximum integration event volume per billing period, or null if unlimited.</summary>
    public long? MaxEventVolumePerMonth { get; init; }

    /// <summary>Gets whether this plan is currently offered for self-service selection.</summary>
    public bool IsActive { get; init; }
}

/// <summary>A single metered dimension's recorded value (mirrors <c>MeteredDimensionDto</c>).</summary>
public sealed class MeteredDimensionDto
{
    /// <summary>Gets the UsageMetricType code this value was recorded for.</summary>
    public string MetricCode { get; init; } = string.Empty;

    /// <summary>Gets the recorded usage value for the period.</summary>
    public decimal RecordedValue { get; init; }

    /// <summary>Gets a human-readable description of which existing platform data source this value was composed from.</summary>
    public string SourceDescription { get; init; } = string.Empty;
}

/// <summary>A tenant's composed usage-metering summary for a single billing period (mirrors <c>TenantUsageMeteringSummaryDto</c>).</summary>
public sealed class TenantUsageMeteringSummaryDto
{
    /// <summary>Gets the tenant this summary was composed for.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the calendar year of the billing period (UTC).</summary>
    public int PeriodYear { get; init; }

    /// <summary>Gets the calendar month of the billing period (UTC).</summary>
    public int PeriodMonth { get; init; }

    /// <summary>Gets one entry per metered usage dimension.</summary>
    public IReadOnlyList<MeteredDimensionDto> Dimensions { get; init; } = [];
}

/// <summary>A single Invoice line item (mirrors <c>InvoiceLineItemDto</c>).</summary>
public sealed class InvoiceLineItemDto
{
    /// <summary>Gets the line item classification (BasePlan/Overage/Discount/Credit).</summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>Gets the human-readable description.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Gets the billable quantity.</summary>
    public decimal Quantity { get; init; }

    /// <summary>Gets the price per unit.</summary>
    public decimal UnitPriceAmount { get; init; }

    /// <summary>Gets the total contribution to the invoice.</summary>
    public decimal Amount { get; init; }
}

/// <summary>Canonical read model for an Invoice (mirrors <c>InvoiceDto</c>).</summary>
public sealed class InvoiceDto
{
    /// <summary>Gets the strong-typed invoice identity.</summary>
    public Guid InvoiceId { get; init; }

    /// <summary>Gets the tenant this invoice belongs to.</summary>
    public Guid TenantId { get; init; }

    /// <summary>Gets the calendar year of the billing period.</summary>
    public int PeriodYear { get; init; }

    /// <summary>Gets the calendar month of the billing period.</summary>
    public int PeriodMonth { get; init; }

    /// <summary>Gets the ISO-4217 currency code for all monetary fields.</summary>
    public string Currency { get; init; } = string.Empty;

    /// <summary>Gets every line item on this invoice, in computation order.</summary>
    public IReadOnlyList<InvoiceLineItemDto> LineItems { get; init; } = [];

    /// <summary>Gets the pre-discount subtotal.</summary>
    public decimal SubtotalAmount { get; init; }

    /// <summary>Gets the final amount due after discounts/credits.</summary>
    public decimal TotalDueAmount { get; init; }

    /// <summary>Gets the current lifecycle status code.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the UTC instant this invoice was generated.</summary>
    public DateTime GeneratedAtUtc { get; init; }

    /// <summary>Gets the UTC instant this invoice was issued, if issued.</summary>
    public DateTime? IssuedAtUtc { get; init; }

    /// <summary>Gets the UTC instant this invoice was paid, if paid.</summary>
    public DateTime? PaidAtUtc { get; init; }

    /// <summary>Gets the successful charge's gateway transaction id, if paid.</summary>
    public string? PaymentProcessorTransactionId { get; init; }
}

/// <summary>A discount or credit to apply to a generated invoice, as an HTTP request field.</summary>
public sealed class DiscountOrCreditRequest
{
    /// <summary>Gets or sets one of "PercentageDiscount", "FixedAmountDiscount", or "Credit".</summary>
    public string Kind { get; set; } = string.Empty;

    /// <summary>Gets or sets the human-readable description shown on the invoice.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the percentage value (0-100), required for "PercentageDiscount".</summary>
    public decimal? PercentageValue { get; set; }

    /// <summary>Gets or sets the fixed amount, required for "FixedAmountDiscount" or "Credit".</summary>
    public decimal? FixedAmount { get; set; }
}

/// <summary>Request body for generating a draft invoice for a tenant's billing period.</summary>
public sealed class GenerateInvoiceRequest
{
    /// <summary>Gets or sets the calendar year of the billing period (UTC).</summary>
    public int PeriodYear { get; set; }

    /// <summary>Gets or sets the calendar month of the billing period (UTC), 1-12.</summary>
    public int PeriodMonth { get; set; }

    /// <summary>Gets or sets the discounts/credits to apply, in application order.</summary>
    public IReadOnlyList<DiscountOrCreditRequest> DiscountsAndCredits { get; set; } = [];
}

/// <summary>Request body for refunding a previously paid invoice.</summary>
public sealed class RefundInvoiceRequest
{
    /// <summary>Gets or sets the reason recorded for the refund/void.</summary>
    public string Reason { get; set; } = string.Empty;
}

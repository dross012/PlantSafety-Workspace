// AUTHOR: PlantSafety Platform Team (Phase 7 §3.3 Billing Engine)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Canonical read model for an Invoice aggregate and its line items.
// SchemaVersion: 1

namespace PlantSafety.Application.Billing.BillingEngine.Models;

/// <summary>Read model for a single Invoice line item.</summary>
/// <param name="Type">Line item classification (BasePlan/Overage/Discount/Credit).</param>
/// <param name="Description">Human-readable description.</param>
/// <param name="Quantity">Billable quantity.</param>
/// <param name="UnitPriceAmount">Price per unit.</param>
/// <param name="Amount">Total contribution to the invoice (positive for charges, negative for discounts/credits).</param>
public sealed record InvoiceLineItemDto(string Type, string Description, decimal Quantity, decimal UnitPriceAmount, decimal Amount);

/// <summary>Canonical read model for an <see cref="Domain.Billing.BillingEngine.Aggregates.Invoice"/>.</summary>
/// <param name="InvoiceId">Strong-typed invoice identity.</param>
/// <param name="TenantId">Tenant this invoice belongs to.</param>
/// <param name="PeriodYear">Calendar year of the billing period.</param>
/// <param name="PeriodMonth">Calendar month of the billing period.</param>
/// <param name="Currency">ISO-4217 currency code for all monetary fields.</param>
/// <param name="LineItems">Every line item on this invoice, in computation order.</param>
/// <param name="SubtotalAmount">Pre-discount subtotal.</param>
/// <param name="TotalDueAmount">Final amount due after discounts/credits.</param>
/// <param name="Status">Current lifecycle status code.</param>
/// <param name="GeneratedAtUtc">UTC instant this invoice was generated.</param>
/// <param name="IssuedAtUtc">UTC instant this invoice was issued, if issued.</param>
/// <param name="PaidAtUtc">UTC instant this invoice was paid, if paid.</param>
/// <param name="PaymentProcessorTransactionId">The successful charge's gateway transaction id, if paid.</param>
public sealed record InvoiceDto(
    Guid InvoiceId,
    Guid TenantId,
    int PeriodYear,
    int PeriodMonth,
    string Currency,
    IReadOnlyList<InvoiceLineItemDto> LineItems,
    decimal SubtotalAmount,
    decimal TotalDueAmount,
    string Status,
    DateTime GeneratedAtUtc,
    DateTime? IssuedAtUtc,
    DateTime? PaidAtUtc,
    string? PaymentProcessorTransactionId);

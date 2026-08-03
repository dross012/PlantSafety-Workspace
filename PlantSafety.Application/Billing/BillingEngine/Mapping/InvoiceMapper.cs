// AUTHOR: PlantSafety Platform Team (Phase 7 §3.3 Billing Engine)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Maps Invoice domain aggregates to their canonical read model.
// SchemaVersion: 1

using PlantSafety.Application.Billing.BillingEngine.Models;
using PlantSafety.Domain.Billing.BillingEngine.Aggregates;

namespace PlantSafety.Application.Billing.BillingEngine.Mapping;

/// <summary>Maps <see cref="Invoice"/> aggregates to <see cref="InvoiceDto"/>.</summary>
public static class InvoiceMapper
{
    /// <summary>Maps an Invoice to its canonical read model.</summary>
    public static InvoiceDto ToDto(Invoice invoice) => new(
        invoice.InvoiceId.Value,
        invoice.TenantId.Value,
        invoice.Period.Year,
        invoice.Period.Month,
        invoice.TotalDue.Currency,
        invoice.LineItems.Select(li => new InvoiceLineItemDto(li.Type.ToString(), li.Description, li.Quantity, li.UnitPrice.Amount, li.Amount.Amount)).ToList(),
        invoice.Subtotal.Amount,
        invoice.TotalDue.Amount,
        invoice.Status.Code,
        invoice.GeneratedAtUtc.Value,
        invoice.IssuedAtUtc?.Value,
        invoice.PaidAtUtc?.Value,
        invoice.PaymentProcessorTransactionId);
}

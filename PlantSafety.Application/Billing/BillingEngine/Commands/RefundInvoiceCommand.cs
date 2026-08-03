// AUTHOR: PlantSafety Platform Team (Phase 7 §3.3 Billing Engine)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Command refunding a previously paid invoice via the IPaymentProcessor
//   extension seam, then voiding the invoice.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Billing.BillingEngine.Commands;

/// <summary>Refunds a previously paid invoice's charge and voids the invoice.</summary>
/// <param name="InvoiceId">Invoice to refund.</param>
/// <param name="Reason">Reason recorded for the refund/void.</param>
public sealed record RefundInvoiceCommand(Guid InvoiceId, string Reason) : IRequest<Result<bool>>;

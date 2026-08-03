// AUTHOR: PlantSafety Platform Team (Phase 7 §3.3 Billing Engine)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Command issuing an Invoice (if still Draft) and attempting to charge the
//   tenant's on-file payment method for the total due, via the IPaymentProcessor extension seam.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Billing.BillingEngine.Models;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Billing.BillingEngine.Commands;

/// <summary>Issues (if Draft) and attempts to charge the given invoice via the configured <c>IPaymentProcessor</c>.</summary>
/// <param name="InvoiceId">Invoice to charge.</param>
public sealed record ChargeInvoiceCommand(Guid InvoiceId) : IRequest<Result<InvoiceDto>>;

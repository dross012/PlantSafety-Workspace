// AUTHOR: PlantSafety Platform Team (Phase 7 §3.3 Billing Engine)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Handler for ChargeInvoiceCommand. Delegates the actual charge to
//   IPaymentProcessor -- the default UnconfiguredPaymentProcessor fails fast rather than
//   fabricating a fake successful charge (see that class's remarks).
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Abstractions.Payments;
using PlantSafety.Application.Billing.BillingEngine.Mapping;
using PlantSafety.Application.Billing.BillingEngine.Models;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Domain.Billing.BillingEngine.Interfaces;
using PlantSafety.Domain.Billing.BillingEngine.ValueObjects.Ids;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Application.Billing.BillingEngine.Commands;

/// <summary>Handles <see cref="ChargeInvoiceCommand"/>.</summary>
public sealed class ChargeInvoiceCommandHandler(IInvoiceRepository invoiceRepository, IPaymentProcessor paymentProcessor)
    : IRequestHandler<ChargeInvoiceCommand, Result<InvoiceDto>>
{
    /// <inheritdoc/>
    public async Task<Result<InvoiceDto>> Handle(ChargeInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await invoiceRepository.GetByIdAsync(InvoiceId.FromGuid(request.InvoiceId), cancellationToken);
        if (invoice is null)
        {
            return Result<InvoiceDto>.Failure(new ApplicationError(
                "Billing.BillingEngine.InvoiceNotFound", $"No invoice found with id '{request.InvoiceId}'.", Guid.NewGuid()));
        }

        var nowUtc = UtcTimestamp.Now();

        if (invoice.Status.Code == "Draft")
            invoice.Issue(nowUtc);

        var chargeResult = await paymentProcessor.ChargeAsync(
            invoice.TenantId.Value, invoice.InvoiceId.Value,
            new Money(invoice.TotalDue.Amount, invoice.TotalDue.Currency),
            idempotencyKey: $"invoice:{invoice.InvoiceId.Value:D}", cancellationToken);

        if (chargeResult.Succeeded)
            invoice.MarkPaid(chargeResult.TransactionId!, UtcTimestamp.Now());
        else
            invoice.MarkPaymentFailed(chargeResult.FailureReason ?? "Payment processor declined the charge.", UtcTimestamp.Now());

        await invoiceRepository.UpdateAsync(invoice, cancellationToken);

        return Result<InvoiceDto>.Success(InvoiceMapper.ToDto(invoice));
    }
}

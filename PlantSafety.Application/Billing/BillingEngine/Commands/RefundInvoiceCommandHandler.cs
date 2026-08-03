// AUTHOR: PlantSafety Platform Team (Phase 7 §3.3 Billing Engine)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Handler for RefundInvoiceCommand.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Abstractions.Payments;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Domain.Billing.BillingEngine.Interfaces;
using PlantSafety.Domain.Billing.BillingEngine.ValueObjects.Ids;
using PlantSafety.SharedKernel.Primitives.ValueTypes;

namespace PlantSafety.Application.Billing.BillingEngine.Commands;

/// <summary>Handles <see cref="RefundInvoiceCommand"/>.</summary>
public sealed class RefundInvoiceCommandHandler(IInvoiceRepository invoiceRepository, IPaymentProcessor paymentProcessor)
    : IRequestHandler<RefundInvoiceCommand, Result<bool>>
{
    /// <inheritdoc/>
    public async Task<Result<bool>> Handle(RefundInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await invoiceRepository.GetByIdAsync(InvoiceId.FromGuid(request.InvoiceId), cancellationToken);
        if (invoice is null)
        {
            return Result<bool>.Failure(new ApplicationError(
                "Billing.BillingEngine.InvoiceNotFound", $"No invoice found with id '{request.InvoiceId}'.", Guid.NewGuid()));
        }

        if (invoice.Status.Code != "Paid" || invoice.PaymentProcessorTransactionId is null)
        {
            return Result<bool>.Failure(new ApplicationError(
                "Billing.BillingEngine.InvoiceNotRefundable", $"Invoice '{request.InvoiceId}' is not in a refundable (Paid) state.", Guid.NewGuid()));
        }

        var refundResult = await paymentProcessor.RefundAsync(
            invoice.TenantId.Value, invoice.PaymentProcessorTransactionId,
            new Money(invoice.TotalDue.Amount, invoice.TotalDue.Currency), cancellationToken);

        if (!refundResult.Succeeded)
        {
            return Result<bool>.Failure(new ApplicationError(
                "Billing.BillingEngine.RefundFailed", refundResult.FailureReason ?? "Payment processor declined the refund.", Guid.NewGuid()));
        }

        invoice.Void(request.Reason, UtcTimestamp.Now());
        await invoiceRepository.UpdateAsync(invoice, cancellationToken);

        return Result<bool>.Success(true);
    }
}

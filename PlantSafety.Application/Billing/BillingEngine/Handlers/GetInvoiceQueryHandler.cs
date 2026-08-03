// AUTHOR: PlantSafety Platform Team (Phase 7 §3.3 Billing Engine)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Handlers for GetInvoiceQuery/GetInvoiceListQuery.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Billing.BillingEngine.Mapping;
using PlantSafety.Application.Billing.BillingEngine.Models;
using PlantSafety.Application.Billing.BillingEngine.Queries;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Domain.Billing.BillingEngine.Interfaces;
using PlantSafety.Domain.Billing.BillingEngine.ValueObjects.Ids;
using TenantId = PlantSafety.Domain.Common.Types.StrongIds.TenantId;

namespace PlantSafety.Application.Billing.BillingEngine.Handlers;

/// <summary>Handles <see cref="GetInvoiceQuery"/>.</summary>
public sealed class GetInvoiceQueryHandler(IInvoiceRepository invoiceRepository) : IRequestHandler<GetInvoiceQuery, Result<InvoiceDto>>
{
    /// <inheritdoc/>
    public async Task<Result<InvoiceDto>> Handle(GetInvoiceQuery request, CancellationToken cancellationToken)
    {
        var invoice = await invoiceRepository.GetByIdAsync(InvoiceId.FromGuid(request.InvoiceId), cancellationToken);
        if (invoice is null)
        {
            return Result<InvoiceDto>.Failure(new ApplicationError(
                "Billing.BillingEngine.InvoiceNotFound", $"No invoice found with id '{request.InvoiceId}'.", Guid.NewGuid()));
        }

        return Result<InvoiceDto>.Success(InvoiceMapper.ToDto(invoice));
    }
}

/// <summary>Handles <see cref="GetInvoiceListQuery"/>.</summary>
public sealed class GetInvoiceListQueryHandler(IInvoiceRepository invoiceRepository) : IRequestHandler<GetInvoiceListQuery, Result<IReadOnlyList<InvoiceDto>>>
{
    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<InvoiceDto>>> Handle(GetInvoiceListQuery request, CancellationToken cancellationToken)
    {
        var invoices = await invoiceRepository.ListByTenantAsync(TenantId.FromGuid(request.TenantId), cancellationToken);
        return Result<IReadOnlyList<InvoiceDto>>.Success(invoices.Select(InvoiceMapper.ToDto).ToList());
    }
}

// AUTHOR: PlantSafety Platform Team (Phase 7 §3.3 Billing Engine)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Handler for GenerateInvoiceCommand. Loads the tenant's already-captured
//   UsageMeteringSnapshot and active SubscriptionPlan, then delegates all money math to
//   InvoiceFactory/InvoiceCalculationRules/UsageOverageCalculator (never re-implemented here).
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Billing.BillingEngine.Mapping;
using PlantSafety.Application.Billing.BillingEngine.Models;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Tenancy.Tenant.Repositories;
using PlantSafety.Domain.Billing.BillingEngine.Factories;
using PlantSafety.Domain.Billing.BillingEngine.Interfaces;
using PlantSafety.Domain.Billing.BillingEngine.ValueObjects;
using PlantSafety.Domain.Billing.Metering.Interfaces;
using PlantSafety.Domain.Billing.Metering.ValueObjects;
using PlantSafety.Domain.Billing.Subscription.Catalog;
using PlantSafety.Domain.Tenancy.Tenant.ValueObjects;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using TenantId = PlantSafety.Domain.Common.Types.StrongIds.TenantId;

namespace PlantSafety.Application.Billing.BillingEngine.Commands;

/// <summary>Handles <see cref="GenerateInvoiceCommand"/>.</summary>
public sealed class GenerateInvoiceCommandHandler(
    ITenantReadRepository tenantReadRepository,
    IUsageMeteringSnapshotRepository snapshotRepository,
    IInvoiceRepository invoiceRepository)
    : IRequestHandler<GenerateInvoiceCommand, Result<InvoiceDto>>
{
    /// <inheritdoc/>
    public async Task<Result<InvoiceDto>> Handle(GenerateInvoiceCommand request, CancellationToken cancellationToken)
    {
        var tenant = await tenantReadRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
        {
            return Result<InvoiceDto>.Failure(new ApplicationError(
                "Billing.BillingEngine.TenantNotFound", $"No tenant found with id '{request.TenantId}'.", Guid.NewGuid()));
        }

        var period = new BillingPeriod(request.PeriodYear, request.PeriodMonth);
        var tenantId = TenantId.FromGuid(request.TenantId);

        var existingInvoice = await invoiceRepository.GetByTenantAndPeriodAsync(tenantId, period, cancellationToken);
        if (existingInvoice is not null)
        {
            return Result<InvoiceDto>.Failure(new ApplicationError(
                "Billing.BillingEngine.InvoiceAlreadyGenerated",
                $"An invoice already exists for tenant '{request.TenantId}' and period '{period}'.", Guid.NewGuid()));
        }

        var snapshot = await snapshotRepository.GetAsync(tenantId, period, cancellationToken);
        if (snapshot is null)
        {
            return Result<InvoiceDto>.Failure(new ApplicationError(
                "Billing.BillingEngine.NoUsageMeteringSnapshot",
                $"No usage metering snapshot has been captured for tenant '{request.TenantId}' and period '{period}'. Capture one via CaptureUsageMeteringSnapshotCommand first.", Guid.NewGuid()));
        }

        var plan = SubscriptionPlanCatalog.Resolve(SubscriptionPlanCode.From(tenant.PlanCode));

        var discountsAndCredits = request.DiscountsAndCredits.Select(ToDomain).ToList();

        var invoice = InvoiceFactory.GenerateForPeriod(tenantId, plan, snapshot, discountsAndCredits, UtcTimestamp.Now());

        await invoiceRepository.AddAsync(invoice, cancellationToken);

        return Result<InvoiceDto>.Success(InvoiceMapper.ToDto(invoice));
    }

    private static DiscountOrCredit ToDomain(DiscountOrCreditRequestDto dto)
    {
        var currency = "USD"; // Matches SubscriptionPlanCatalog's DefaultCurrency for every plan today.

        return dto.Kind switch
        {
            "PercentageDiscount" => DiscountOrCredit.Percentage(dto.Description, dto.PercentageValue
                ?? throw new ArgumentException("PercentageDiscount requires PercentageValue.")),
            "FixedAmountDiscount" => DiscountOrCredit.FixedDiscount(dto.Description, new Money(dto.FixedAmount
                ?? throw new ArgumentException("FixedAmountDiscount requires FixedAmount."), currency)),
            "Credit" => DiscountOrCredit.AccountCredit(dto.Description, new Money(dto.FixedAmount
                ?? throw new ArgumentException("Credit requires FixedAmount."), currency)),
            _ => throw new ArgumentException($"Unknown DiscountOrCreditRequestDto.Kind '{dto.Kind}'."),
        };
    }
}

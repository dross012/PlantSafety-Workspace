// AUTHOR: PlantSafety Platform Team (Phase 7 §3.1 Usage Metering)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Handler for CaptureUsageMeteringSnapshotCommand. Delegates the actual
//   composition of the six metering data sources to GetTenantUsageMeteringSummaryQuery (intra-
//   module MediatR dispatch, fully permitted -- see IReportSectionProvider remarks), then persists
//   the result as an immutable UsageMeteringSnapshot domain aggregate.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Billing.Metering.Queries;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Domain.Billing.Metering.Aggregates;
using PlantSafety.Domain.Billing.Metering.Enums;
using PlantSafety.Domain.Billing.Metering.Interfaces;
using PlantSafety.Domain.Billing.Metering.ValueObjects;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using TenantId = PlantSafety.Domain.Common.Types.StrongIds.TenantId;

namespace PlantSafety.Application.Billing.Metering.Commands;

/// <summary>Handles <see cref="CaptureUsageMeteringSnapshotCommand"/>.</summary>
public sealed class CaptureUsageMeteringSnapshotCommandHandler(
    ISender sender, IUsageMeteringSnapshotRepository snapshotRepository)
    : IRequestHandler<CaptureUsageMeteringSnapshotCommand, Result<Guid>>
{
    /// <inheritdoc/>
    public async Task<Result<Guid>> Handle(CaptureUsageMeteringSnapshotCommand request, CancellationToken cancellationToken)
    {
        var summaryResult = await sender.Send(
            new GetTenantUsageMeteringSummaryQuery(request.TenantId, request.PeriodYear, request.PeriodMonth), cancellationToken);

        if (summaryResult.IsFailure)
        {
            return Result<Guid>.Failure(new ApplicationError(
                "Billing.Metering.SummaryCompositionFailed", "Unable to compose the tenant's usage-metering summary.", Guid.NewGuid()));
        }

        var recordedValues = summaryResult.Value.Dimensions.ToDictionary(
            d => UsageMetricType.From(d.MetricCode), d => d.RecordedValue);

        var period = new BillingPeriod(request.PeriodYear, request.PeriodMonth);
        var snapshot = UsageMeteringSnapshot.Capture(
            TenantId.FromGuid(request.TenantId), period, recordedValues, UtcTimestamp.Now());

        await snapshotRepository.SaveAsync(snapshot, cancellationToken);

        return Result<Guid>.Success(snapshot.UsageMeteringSnapshotId.Value);
    }
}

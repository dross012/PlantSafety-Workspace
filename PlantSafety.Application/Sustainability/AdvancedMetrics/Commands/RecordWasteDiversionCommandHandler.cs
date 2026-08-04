// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Application
//  File:           RecordWasteDiversionCommandHandler.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Handler for RecordWasteDiversionCommand. Get-or-creates the open
//                  AdvancedMetricsLedger for the tenant/site/reporting period, then records the
//                  waste diversion interval on it via the domain aggregate method.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Sustainability.AdvancedMetrics.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Sustainability.AdvancedMetrics.Commands;

/// <summary>Handles <see cref="RecordWasteDiversionCommand"/>.</summary>
public sealed class RecordWasteDiversionCommandHandler : IRequestHandler<RecordWasteDiversionCommand, Result<RecordWasteDiversionResult>>
{
    private readonly IAdvancedMetricsLedgerRepository _repository;
    private readonly ILogger<RecordWasteDiversionCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordWasteDiversionCommandHandler(IAdvancedMetricsLedgerRepository repository, ILogger<RecordWasteDiversionCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RecordWasteDiversionResult>> Handle(RecordWasteDiversionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var requesterId = UserId.FromGuid(request.RequesterUserId);
            var periodStart = UtcTimestamp.From(request.ReportingPeriodStartUtc);
            var periodEnd = UtcTimestamp.From(request.ReportingPeriodEndUtc);

            var ledger = await _repository.GetOpenLedgerAsync(request.TenantId, request.SiteId, periodStart, periodEnd, cancellationToken);
            var isNewLedger = ledger is null;
            ledger ??= PlantSafety.Domain.Sustainability.AdvancedMetrics.Aggregates.AdvancedMetricsLedger.Create(
                createdBy: requesterId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(requesterId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                reportingPeriodStartUtc: periodStart,
                reportingPeriodEndUtc: periodEnd);

            var record = ledger.RecordWasteDiversion(
                totalWasteKg: request.TotalWasteKg,
                divertedWasteKg: request.DivertedWasteKg,
                methodologyVersion: request.MethodologyVersion,
                intervalEndAtUtc: UtcTimestamp.From(request.IntervalEndAtUtc),
                recordedAtUtc: UtcTimestamp.Now(),
                recordedBy: requesterId,
                actor: PerformedByActor.User(requesterId),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            if (isNewLedger)
                await _repository.AddAsync(ledger, cancellationToken);
            else
                await _repository.UpdateAsync(ledger, cancellationToken);

            return Result<RecordWasteDiversionResult>.Success(new RecordWasteDiversionResult(
                ledger.AdvancedMetricsLedgerId.Value, record.WasteDiversionRecordId.Value, record.DiversionRatePercent));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record waste diversion for Tenant {TenantId}", request.TenantId);

            return Result<RecordWasteDiversionResult>.Failure(new ApplicationError(
                "Sustainability.AdvancedMetrics.RecordWasteDiversionFailed",
                "Unable to record the waste diversion interval at this time.",
                request.CorrelationId));
        }
    }
}

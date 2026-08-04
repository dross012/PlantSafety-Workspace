// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Application
//  File:           RecordEnergyIntensityMetricCommandHandler.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Handler for RecordEnergyIntensityMetricCommand. Get-or-creates the open
//                  AdvancedMetricsLedger for the tenant/site/reporting period, then records the
//                  metric on it via the domain aggregate method. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Sustainability.AdvancedMetrics.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Sustainability.AdvancedMetrics.Commands;

/// <summary>Handles <see cref="RecordEnergyIntensityMetricCommand"/>.</summary>
public sealed class RecordEnergyIntensityMetricCommandHandler : IRequestHandler<RecordEnergyIntensityMetricCommand, Result<RecordEnergyIntensityMetricResult>>
{
    private readonly IAdvancedMetricsLedgerRepository _repository;
    private readonly ILogger<RecordEnergyIntensityMetricCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordEnergyIntensityMetricCommandHandler(IAdvancedMetricsLedgerRepository repository, ILogger<RecordEnergyIntensityMetricCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RecordEnergyIntensityMetricResult>> Handle(RecordEnergyIntensityMetricCommand request, CancellationToken cancellationToken)
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

            var metric = ledger.RecordEnergyIntensityMetric(
                energyConsumedKwh: request.EnergyConsumedKwh,
                outputUnits: request.OutputUnits,
                methodologyVersion: request.MethodologyVersion,
                readingAtUtc: UtcTimestamp.From(request.ReadingAtUtc),
                recordedAtUtc: UtcTimestamp.Now(),
                recordedBy: requesterId,
                actor: PerformedByActor.User(requesterId),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            if (isNewLedger)
                await _repository.AddAsync(ledger, cancellationToken);
            else
                await _repository.UpdateAsync(ledger, cancellationToken);

            return Result<RecordEnergyIntensityMetricResult>.Success(new RecordEnergyIntensityMetricResult(
                ledger.AdvancedMetricsLedgerId.Value, metric.EnergyIntensityMetricId.Value, metric.IntensityValue));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record energy intensity metric for Tenant {TenantId}", request.TenantId);

            return Result<RecordEnergyIntensityMetricResult>.Failure(new ApplicationError(
                "Sustainability.AdvancedMetrics.RecordEnergyIntensityMetricFailed",
                "Unable to record the energy intensity metric at this time.",
                request.CorrelationId));
        }
    }
}

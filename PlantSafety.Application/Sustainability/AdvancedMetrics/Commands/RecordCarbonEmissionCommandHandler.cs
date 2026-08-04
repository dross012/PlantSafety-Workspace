// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Application
//  File:           RecordCarbonEmissionCommandHandler.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Handler for RecordCarbonEmissionCommand. Get-or-creates the open
//                  AdvancedMetricsLedger for the tenant/site/reporting period, then records the
//                  emission on it via the domain aggregate method. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Sustainability.AdvancedMetrics.Enums;
using PlantSafety.Domain.Sustainability.AdvancedMetrics.Interfaces;
using PlantSafety.Domain.Sustainability.AdvancedMetrics.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Sustainability.AdvancedMetrics.Commands;

/// <summary>Handles <see cref="RecordCarbonEmissionCommand"/>.</summary>
public sealed class RecordCarbonEmissionCommandHandler : IRequestHandler<RecordCarbonEmissionCommand, Result<RecordCarbonEmissionResult>>
{
    private readonly IAdvancedMetricsLedgerRepository _repository;
    private readonly ILogger<RecordCarbonEmissionCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordCarbonEmissionCommandHandler(IAdvancedMetricsLedgerRepository repository, ILogger<RecordCarbonEmissionCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RecordCarbonEmissionResult>> Handle(RecordCarbonEmissionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!Enum.TryParse<GhgScope>(request.Scope, ignoreCase: true, out var scope))
            {
                return Result<RecordCarbonEmissionResult>.Failure(new ApplicationError(
                    "Sustainability.AdvancedMetrics.InvalidScope",
                    $"'{request.Scope}' is not a recognized GHG scope.",
                    request.CorrelationId,
                    ErrorSeverity.Validation));
            }

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

            var record = ledger.RecordCarbonEmission(
                scope: scope,
                activityDescription: request.ActivityDescription,
                emissionsMetricTonsCo2e: request.EmissionsMetricTonsCo2e,
                methodologyVersion: request.MethodologyVersion,
                activityAtUtc: UtcTimestamp.From(request.ActivityAtUtc),
                recordedAtUtc: UtcTimestamp.Now(),
                recordedBy: requesterId,
                actor: PerformedByActor.User(requesterId),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            if (isNewLedger)
                await _repository.AddAsync(ledger, cancellationToken);
            else
                await _repository.UpdateAsync(ledger, cancellationToken);

            return Result<RecordCarbonEmissionResult>.Success(new RecordCarbonEmissionResult(
                ledger.AdvancedMetricsLedgerId.Value, record.CarbonEmissionRecordId.Value, record.EmissionsMetricTonsCo2e));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record carbon emission for Tenant {TenantId}", request.TenantId);

            return Result<RecordCarbonEmissionResult>.Failure(new ApplicationError(
                "Sustainability.AdvancedMetrics.RecordCarbonEmissionFailed",
                "Unable to record the carbon emission at this time.",
                request.CorrelationId));
        }
    }
}

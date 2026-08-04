// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability.AdvancedMetrics
//  Layer:          Application
//  File:           SetSustainabilityTargetCommandHandler.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Handler for SetSustainabilityTargetCommand. Get-or-creates the open
//                  AdvancedMetricsLedger for the tenant/site/reporting period, then sets the target
//                  on it via the domain aggregate method. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Sustainability.AdvancedMetrics.Enums;
using PlantSafety.Domain.Sustainability.AdvancedMetrics.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Sustainability.AdvancedMetrics.Commands;

/// <summary>Handles <see cref="SetSustainabilityTargetCommand"/>.</summary>
public sealed class SetSustainabilityTargetCommandHandler : IRequestHandler<SetSustainabilityTargetCommand, Result<SetSustainabilityTargetResult>>
{
    private readonly IAdvancedMetricsLedgerRepository _repository;
    private readonly ILogger<SetSustainabilityTargetCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public SetSustainabilityTargetCommandHandler(IAdvancedMetricsLedgerRepository repository, ILogger<SetSustainabilityTargetCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<SetSustainabilityTargetResult>> Handle(SetSustainabilityTargetCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!Enum.TryParse<SustainabilityTargetType>(request.TargetType, ignoreCase: true, out var targetType))
            {
                return Result<SetSustainabilityTargetResult>.Failure(new ApplicationError(
                    "Sustainability.AdvancedMetrics.InvalidTargetType",
                    $"'{request.TargetType}' is not a recognized sustainability target type.",
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

            var target = ledger.SetSustainabilityTarget(
                targetType: targetType,
                baselineValue: request.BaselineValue,
                targetValue: request.TargetValue,
                targetDateUtc: UtcTimestamp.From(request.TargetDateUtc),
                setAtUtc: UtcTimestamp.Now(),
                setBy: requesterId,
                actor: PerformedByActor.User(requesterId),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            if (isNewLedger)
                await _repository.AddAsync(ledger, cancellationToken);
            else
                await _repository.UpdateAsync(ledger, cancellationToken);

            return Result<SetSustainabilityTargetResult>.Success(new SetSustainabilityTargetResult(
                ledger.AdvancedMetricsLedgerId.Value, target.SustainabilityTargetId.Value, target.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set sustainability target for Tenant {TenantId}", request.TenantId);

            return Result<SetSustainabilityTargetResult>.Failure(new ApplicationError(
                "Sustainability.AdvancedMetrics.SetSustainabilityTargetFailed",
                "Unable to set the sustainability target at this time.",
                request.CorrelationId));
        }
    }
}

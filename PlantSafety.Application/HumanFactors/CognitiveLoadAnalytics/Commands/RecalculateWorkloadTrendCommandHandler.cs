// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Application
//  File:           RecalculateWorkloadTrendCommandHandler.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Handler for RecalculateWorkloadTrendCommand. Loads the target ledger by
//                  identity and recalculates its workload trend for the requested window via the
//                  domain aggregate method. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Interfaces;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Commands;

/// <summary>Handles <see cref="RecalculateWorkloadTrendCommand"/>.</summary>
public sealed class RecalculateWorkloadTrendCommandHandler : IRequestHandler<RecalculateWorkloadTrendCommand, Result<RecalculateWorkloadTrendResult>>
{
    private readonly ICognitiveLoadLedgerRepository _repository;
    private readonly ILogger<RecalculateWorkloadTrendCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecalculateWorkloadTrendCommandHandler(ICognitiveLoadLedgerRepository repository, ILogger<RecalculateWorkloadTrendCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RecalculateWorkloadTrendResult>> Handle(RecalculateWorkloadTrendCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var ledger = await _repository.GetByIdAsync(CognitiveLoadLedgerId.FromGuid(request.CognitiveLoadLedgerId), request.TenantId, cancellationToken);
            if (ledger is null)
            {
                return Result<RecalculateWorkloadTrendResult>.Failure(new ApplicationError(
                    "HumanFactors.CognitiveLoadAnalytics.CognitiveLoadLedger.NotFound",
                    $"CognitiveLoadLedger {request.CognitiveLoadLedgerId} was not found.",
                    request.CorrelationId,
                    ErrorSeverity.NotFound));
            }

            var requesterId = UserId.FromGuid(request.RequesterUserId);

            var record = ledger.RecalculateWorkloadTrend(
                periodStartUtc: UtcTimestamp.From(request.PeriodStartUtc),
                periodEndUtc: UtcTimestamp.From(request.PeriodEndUtc),
                recalculatedAtUtc: UtcTimestamp.Now(),
                recalculatedBy: requesterId,
                actor: PerformedByActor.User(requesterId),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(ledger, cancellationToken);

            return Result<RecalculateWorkloadTrendResult>.Success(new RecalculateWorkloadTrendResult(
                ledger.CognitiveLoadLedgerId.Value, record.WorkloadTrendRecordId.Value,
                record.MeanCognitiveLoadScore, record.OverloadEventCount, record.ReadingCount));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to recalculate workload trend for CognitiveLoadLedger {CognitiveLoadLedgerId}", request.CognitiveLoadLedgerId);

            return Result<RecalculateWorkloadTrendResult>.Failure(new ApplicationError(
                "HumanFactors.CognitiveLoadAnalytics.RecalculateWorkloadTrendFailed",
                "Unable to recalculate the workload trend at this time.",
                request.CorrelationId));
        }
    }
}

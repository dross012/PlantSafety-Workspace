// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         HumanFactors.CognitiveLoadAnalytics
//  Layer:          Application
//  File:           RecordCognitiveLoadReadingCommandHandler.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Handler for RecordCognitiveLoadReadingCommand. Get-or-creates the open
//                  CognitiveLoadLedger for the tenant/site/shift, then records the reading on it
//                  via the domain aggregate method. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Enums;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Interfaces;
using PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Commands;

/// <summary>Handles <see cref="RecordCognitiveLoadReadingCommand"/>.</summary>
public sealed class RecordCognitiveLoadReadingCommandHandler : IRequestHandler<RecordCognitiveLoadReadingCommand, Result<RecordCognitiveLoadReadingResult>>
{
    private readonly ICognitiveLoadLedgerRepository _repository;
    private readonly ILogger<RecordCognitiveLoadReadingCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordCognitiveLoadReadingCommandHandler(ICognitiveLoadLedgerRepository repository, ILogger<RecordCognitiveLoadReadingCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RecordCognitiveLoadReadingResult>> Handle(RecordCognitiveLoadReadingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!Enum.TryParse<ShiftType>(request.ShiftType, ignoreCase: true, out var shiftType))
            {
                return Result<RecordCognitiveLoadReadingResult>.Failure(new ApplicationError(
                    "HumanFactors.CognitiveLoadAnalytics.InvalidShiftType",
                    $"'{request.ShiftType}' is not a recognized shift type.",
                    request.CorrelationId,
                    ErrorSeverity.Validation));
            }

            var requesterId = UserId.FromGuid(request.RequesterUserId);
            var shiftDateUtc = UtcTimestamp.From(request.ShiftDateUtc);

            var ledger = await _repository.GetOpenLedgerAsync(request.TenantId, request.SiteId, shiftType, shiftDateUtc, cancellationToken);
            var isNewLedger = ledger is null;
            ledger ??= PlantSafety.Domain.HumanFactors.CognitiveLoadAnalytics.Aggregates.CognitiveLoadLedger.Create(
                createdBy: requesterId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(requesterId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                shiftType: shiftType,
                shiftDateUtc: shiftDateUtc);

            var reading = ledger.RecordCognitiveLoadReading(
                workerUserId: UserId.FromGuid(request.WorkerUserId),
                taskComplexityProfileId: request.TaskComplexityProfileId is { } profileId ? TaskComplexityProfileId.FromGuid(profileId) : null,
                cognitiveLoadScore: request.CognitiveLoadScore,
                measurementMethod: request.MeasurementMethod,
                measuredAtUtc: UtcTimestamp.From(request.MeasuredAtUtc),
                recordedAtUtc: UtcTimestamp.Now(),
                recordedBy: requesterId,
                actor: PerformedByActor.User(requesterId),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            if (isNewLedger)
                await _repository.AddAsync(ledger, cancellationToken);
            else
                await _repository.UpdateAsync(ledger, cancellationToken);

            return Result<RecordCognitiveLoadReadingResult>.Success(new RecordCognitiveLoadReadingResult(
                ledger.CognitiveLoadLedgerId.Value, reading.CognitiveLoadReadingId.Value, reading.CognitiveLoadScore,
                reading.WorkloadLevel.ToString(), reading.IsOverload));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record cognitive load reading for Tenant {TenantId}", request.TenantId);

            return Result<RecordCognitiveLoadReadingResult>.Failure(new ApplicationError(
                "HumanFactors.CognitiveLoadAnalytics.RecordCognitiveLoadReadingFailed",
                "Unable to record the cognitive load reading at this time.",
                request.CorrelationId));
        }
    }
}

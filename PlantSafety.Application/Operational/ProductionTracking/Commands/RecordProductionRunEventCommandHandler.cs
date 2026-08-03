// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.ProductionTracking
//  Layer:          Application
//  File:           RecordProductionRunEventCommandHandler.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Handler for RecordProductionRunEventCommand. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Domain.Operational.ProductionTracking.Interfaces;
using PlantSafety.Domain.Operational.ProductionTracking.ValueObjects.Ids;

namespace PlantSafety.Application.Operational.ProductionTracking.Commands;

/// <summary>Handles <see cref="RecordProductionRunEventCommand"/>.</summary>
public sealed class RecordProductionRunEventCommandHandler(IProductionRunRepository repository, ILogger<RecordProductionRunEventCommandHandler> logger)
    : IRequestHandler<RecordProductionRunEventCommand, Result<ProductionRunEventResult>>
{
    /// <inheritdoc/>
    public async Task<Result<ProductionRunEventResult>> Handle(RecordProductionRunEventCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = ProductionRunId.FromGuid(request.RunId);
            var aggregate = await repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<ProductionRunEventResult>.Failure(new ApplicationError(
                    "Operational.ProductionTracking.NotFound",
                    "The requested production run could not be found.",
                    request.RunId,
                    ErrorSeverity.NotFound));
            }

            var requesterId = UserId.FromGuid(request.RequestedByUserId);

            aggregate.RecordEvent(
                runEventType: request.RunEventType,
                description: request.Description,
                isDowntime: request.IsDowntime,
                downtimeReasonCode: request.DowntimeReasonCode,
                downtimeMinutes: request.DowntimeMinutes,
                goodUnitsDelta: request.GoodUnitsDelta,
                scrapUnitsDelta: request.ScrapUnitsDelta,
                occurredAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(requesterId),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<ProductionRunEventResult>.Success(new ProductionRunEventResult(
                aggregate.ProductionRunId.Value,
                aggregate.GoodUnitCount,
                aggregate.ScrapUnitCount));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to record event for ProductionRun {RunId}", request.RunId);

            return Result<ProductionRunEventResult>.Failure(new ApplicationError(
                "Operational.ProductionTracking.RecordEventFailed",
                "Unable to record the production run event at this time.",
                Guid.NewGuid()));
        }
    }
}

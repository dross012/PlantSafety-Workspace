// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.ProductionTracking
//  Layer:          Application
//  File:           AddProductionBatchCommandHandler.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Handler for AddProductionBatchCommand. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Domain.Operational.ProductionTracking.Enums;
using PlantSafety.Domain.Operational.ProductionTracking.Interfaces;
using PlantSafety.Domain.Operational.ProductionTracking.ValueObjects.Ids;

namespace PlantSafety.Application.Operational.ProductionTracking.Commands;

/// <summary>Handles <see cref="AddProductionBatchCommand"/>.</summary>
public sealed class AddProductionBatchCommandHandler(IProductionRunRepository repository, ILogger<AddProductionBatchCommandHandler> logger)
    : IRequestHandler<AddProductionBatchCommand, Result<AddProductionBatchResult>>
{
    /// <inheritdoc/>
    public async Task<Result<AddProductionBatchResult>> Handle(AddProductionBatchCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = ProductionRunId.FromGuid(request.RunId);
            var aggregate = await repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<AddProductionBatchResult>.Failure(new ApplicationError(
                    "Operational.ProductionTracking.NotFound",
                    "The requested production run could not be found.",
                    request.RunId,
                    ErrorSeverity.NotFound));
            }

            if (!Enum.TryParse<BatchStatus>(request.Status, ignoreCase: true, out var status))
            {
                return Result<AddProductionBatchResult>.Failure(new ApplicationError(
                    "Operational.ProductionTracking.InvalidBatchStatus",
                    $"'{request.Status}' is not a recognised batch status.",
                    request.CorrelationId,
                    ErrorSeverity.Validation));
            }

            var requesterId = UserId.FromGuid(request.RequestedByUserId);

            var batchId = aggregate.AddBatch(
                batchCode: request.BatchCode,
                status: status,
                startedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(requesterId),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<AddProductionBatchResult>.Success(new AddProductionBatchResult(
                batchId.Value,
                aggregate.ProductionRunId.Value,
                request.BatchCode));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to add batch to ProductionRun {RunId}", request.RunId);

            return Result<AddProductionBatchResult>.Failure(new ApplicationError(
                "Operational.ProductionTracking.AddBatchFailed",
                "Unable to add the batch to the production run at this time.",
                Guid.NewGuid()));
        }
    }
}

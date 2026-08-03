// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.ProductionTracking
//  Layer:          Application
//  File:           CompleteProductionRunCommandHandler.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Handler for CompleteProductionRunCommand. Additive-only evolution.
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

/// <summary>Handles <see cref="CompleteProductionRunCommand"/>.</summary>
public sealed class CompleteProductionRunCommandHandler(IProductionRunRepository repository, ILogger<CompleteProductionRunCommandHandler> logger)
    : IRequestHandler<CompleteProductionRunCommand, Result<ProductionRunStatusResult>>
{
    /// <inheritdoc/>
    public async Task<Result<ProductionRunStatusResult>> Handle(CompleteProductionRunCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = ProductionRunId.FromGuid(request.RunId);
            var aggregate = await repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<ProductionRunStatusResult>.Failure(new ApplicationError(
                    "Operational.ProductionTracking.NotFound",
                    "The requested production run could not be found.",
                    request.RunId,
                    ErrorSeverity.NotFound));
            }

            var requesterId = UserId.FromGuid(request.RequestedByUserId);

            aggregate.End(
                succeeded: request.Succeeded,
                endedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(requesterId),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<ProductionRunStatusResult>.Success(new ProductionRunStatusResult(
                aggregate.ProductionRunId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to complete ProductionRun {RunId}", request.RunId);

            return Result<ProductionRunStatusResult>.Failure(new ApplicationError(
                "Operational.ProductionTracking.CompleteRunFailed",
                "Unable to complete the production run at this time.",
                Guid.NewGuid()));
        }
    }
}

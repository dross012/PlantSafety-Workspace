// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Operational.WorkOrders
//  Layer:          Application
//  File:           AdjustPartStockCommandHandler.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Handler for AdjustPartStockCommand. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Domain.Operational.WorkOrders.Exceptions;
using PlantSafety.Domain.Operational.WorkOrders.Interfaces;
using PlantSafety.Domain.Operational.WorkOrders.ValueObjects.Ids;

namespace PlantSafety.Application.Operational.WorkOrders.Commands;

/// <summary>Handles <see cref="AdjustPartStockCommand"/>.</summary>
public sealed class AdjustPartStockCommandHandler : IRequestHandler<AdjustPartStockCommand, Result<PartStockResult>>
{
    private readonly IPartRepository _repository;
    private readonly ILogger<AdjustPartStockCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public AdjustPartStockCommandHandler(IPartRepository repository, ILogger<AdjustPartStockCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PartStockResult>> Handle(AdjustPartStockCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = PartId.FromGuid(request.PartId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<PartStockResult>.Failure(new ApplicationError(
                    "Operational.WorkOrders.Part.NotFound",
                    "The requested part could not be found.",
                    request.PartId,
                    ErrorSeverity.NotFound));
            }

            var requesterId = UserId.FromGuid(request.RequestedByUserId);

            aggregate.AdjustStock(
                deltaQuantity: request.DeltaQuantity,
                reason: request.Reason,
                adjustedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(requesterId),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<PartStockResult>.Success(new PartStockResult(
                aggregate.PartId.Value,
                aggregate.QuantityOnHand));
        }
        catch (PartInsufficientStockException ex)
        {
            return Result<PartStockResult>.Failure(new ApplicationError(
                "Operational.WorkOrders.Part.InsufficientStock",
                ex.Message,
                request.CorrelationId,
                ErrorSeverity.Domain));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to adjust stock for Part {PartId}", request.PartId);

            return Result<PartStockResult>.Failure(new ApplicationError(
                "Operational.WorkOrders.Part.AdjustStockFailed",
                "Unable to adjust part stock at this time.",
                Guid.NewGuid()));
        }
    }
}

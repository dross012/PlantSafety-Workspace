// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for ReceivePPEStockCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.PPE.PPEInventory.Interfaces;
using PlantSafety.Domain.PPE.PPEInventory.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PPE.Commands;

/// <summary>Handles <see cref="ReceivePPEStockCommand"/>.</summary>
public sealed class ReceivePPEStockCommandHandler : IRequestHandler<ReceivePPEStockCommand, Result<PPEInventoryActionResult>>
{
    private readonly IPPEInventoryRepository _repository;
    private readonly ILogger<ReceivePPEStockCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ReceivePPEStockCommandHandler(IPPEInventoryRepository repository, ILogger<ReceivePPEStockCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PPEInventoryActionResult>> Handle(ReceivePPEStockCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = PPEInventoryId.FromGuid(request.PPEInventoryId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<PPEInventoryActionResult>.Failure(new ApplicationError(
                    "PPE.PPEInventory.NotFound",
                    "The requested PPE inventory item could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.ReceiveStock(
                quantity: request.Quantity,
                receivedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<PPEInventoryActionResult>.Success(new PPEInventoryActionResult(
                aggregate.PPEInventoryId.Value,
                aggregate.Status.ToString(),
                aggregate.AvailableQuantity));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to receive stock for PPEInventory {PPEInventoryId}", request.PPEInventoryId);

            return Result<PPEInventoryActionResult>.Failure(new ApplicationError(
                "PPE.PPEInventory.ReceiveStockFailed",
                "Unable to receive stock at this time.",
                Guid.NewGuid()));
        }
    }
}

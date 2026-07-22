// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for IssuePPEUnitsCommand.
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

/// <summary>Handles <see cref="IssuePPEUnitsCommand"/>.</summary>
public sealed class IssuePPEUnitsCommandHandler : IRequestHandler<IssuePPEUnitsCommand, Result<PPEInventoryActionResult>>
{
    private readonly IPPEInventoryRepository _repository;
    private readonly ILogger<IssuePPEUnitsCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public IssuePPEUnitsCommandHandler(IPPEInventoryRepository repository, ILogger<IssuePPEUnitsCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PPEInventoryActionResult>> Handle(IssuePPEUnitsCommand request, CancellationToken cancellationToken)
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

            aggregate.IssueUnits(
                quantity: request.Quantity,
                reorderThreshold: request.ReorderThreshold,
                issuedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<PPEInventoryActionResult>.Success(new PPEInventoryActionResult(
                aggregate.PPEInventoryId.Value,
                aggregate.Status.ToString(),
                aggregate.AvailableQuantity));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Rejected PPE issuance for PPEInventory {PPEInventoryId}: insufficient stock", request.PPEInventoryId);

            return Result<PPEInventoryActionResult>.Failure(new ApplicationError(
                "PPE.PPEInventory.InsufficientStock",
                "Insufficient available stock to issue the requested quantity.",
                Guid.NewGuid()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to issue units for PPEInventory {PPEInventoryId}", request.PPEInventoryId);

            return Result<PPEInventoryActionResult>.Failure(new ApplicationError(
                "PPE.PPEInventory.IssueUnitsFailed",
                "Unable to issue PPE units at this time.",
                Guid.NewGuid()));
        }
    }
}

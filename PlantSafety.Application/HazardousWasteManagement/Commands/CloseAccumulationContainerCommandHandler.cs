// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for CloseAccumulationContainerCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.HazardousWasteManagement.WasteContainers.Interfaces;
using PlantSafety.Domain.HazardousWasteManagement.WasteContainers.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.HazardousWasteManagement.Commands;

/// <summary>Handles <see cref="CloseAccumulationContainerCommand"/>.</summary>
public sealed class CloseAccumulationContainerCommandHandler : IRequestHandler<CloseAccumulationContainerCommand, Result<WasteContainerActionResult>>
{
    private readonly IWasteContainerRepository _repository;
    private readonly ILogger<CloseAccumulationContainerCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CloseAccumulationContainerCommandHandler(IWasteContainerRepository repository, ILogger<CloseAccumulationContainerCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<WasteContainerActionResult>> Handle(CloseAccumulationContainerCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = WasteContainerId.FromGuid(request.WasteContainerId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<WasteContainerActionResult>.Failure(new ApplicationError(
                    "HazardousWasteManagement.WasteContainer.NotFound",
                    "The requested WasteContainer record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Close(
                UtcTimestamp.Now(), PerformedByActor.User(performedBy),
                new CorrelationId(request.CorrelationId), new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<WasteContainerActionResult>.Success(new WasteContainerActionResult(
                aggregate.WasteContainerId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute CloseAccumulationContainer for WasteContainer {WasteContainerId}", request.WasteContainerId);

            return Result<WasteContainerActionResult>.Failure(new ApplicationError(
                "HazardousWasteManagement.WasteContainer.CloseFailed",
                "Unable to close the accumulation container at this time.",
                Guid.NewGuid()));
        }
    }
}

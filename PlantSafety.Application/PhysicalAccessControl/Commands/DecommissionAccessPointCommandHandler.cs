// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for DecommissionAccessPointCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Interfaces;
using PlantSafety.Domain.PhysicalAccessControl.AccessPoints.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PhysicalAccessControl.Commands;

/// <summary>Handles <see cref="DecommissionAccessPointCommand"/>.</summary>
public sealed class DecommissionAccessPointCommandHandler : IRequestHandler<DecommissionAccessPointCommand, Result<AccessPointActionResult>>
{
    private readonly IAccessPointRepository _repository;
    private readonly ILogger<DecommissionAccessPointCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public DecommissionAccessPointCommandHandler(IAccessPointRepository repository, ILogger<DecommissionAccessPointCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AccessPointActionResult>> Handle(DecommissionAccessPointCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = AccessPointId.FromGuid(request.AccessPointId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<AccessPointActionResult>.Failure(new ApplicationError(
                    "PhysicalAccessControl.AccessPoint.NotFound",
                    "The requested AccessPoint record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Decommission(
                UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<AccessPointActionResult>.Success(new AccessPointActionResult(
                aggregate.AccessPointId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute Decommission for AccessPoint {AccessPointId}", request.AccessPointId);

            return Result<AccessPointActionResult>.Failure(new ApplicationError(
                "PhysicalAccessControl.AccessPoint.DecommissionFailed",
                "Unable to decommission the AccessPoint record at this time.",
                Guid.NewGuid()));
        }
    }
}

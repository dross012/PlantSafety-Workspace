// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for RestrictSafetyZoneCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.PhysicalAccessControl.SafetyZones.Interfaces;
using PlantSafety.Domain.PhysicalAccessControl.SafetyZones.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PhysicalAccessControl.Commands;

/// <summary>Handles <see cref="RestrictSafetyZoneCommand"/>.</summary>
public sealed class RestrictSafetyZoneCommandHandler : IRequestHandler<RestrictSafetyZoneCommand, Result<SafetyZoneActionResult>>
{
    private readonly ISafetyZoneRepository _repository;
    private readonly ILogger<RestrictSafetyZoneCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RestrictSafetyZoneCommandHandler(ISafetyZoneRepository repository, ILogger<RestrictSafetyZoneCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<SafetyZoneActionResult>> Handle(RestrictSafetyZoneCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = SafetyZoneId.FromGuid(request.SafetyZoneId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<SafetyZoneActionResult>.Failure(new ApplicationError(
                    "PhysicalAccessControl.SafetyZone.NotFound",
                    "The requested SafetyZone record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Restrict(
                UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<SafetyZoneActionResult>.Success(new SafetyZoneActionResult(
                aggregate.SafetyZoneId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute Restrict for SafetyZone {SafetyZoneId}", request.SafetyZoneId);

            return Result<SafetyZoneActionResult>.Failure(new ApplicationError(
                "PhysicalAccessControl.SafetyZone.RestrictFailed",
                "Unable to restrict the SafetyZone record at this time.",
                Guid.NewGuid()));
        }
    }
}

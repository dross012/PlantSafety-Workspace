// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for ActivateRestrictedZonePolicyCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.Interfaces;
using PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PhysicalAccessControl.Commands;

/// <summary>Handles <see cref="ActivateRestrictedZonePolicyCommand"/>.</summary>
public sealed class ActivateRestrictedZonePolicyCommandHandler : IRequestHandler<ActivateRestrictedZonePolicyCommand, Result<RestrictedZonePolicyActionResult>>
{
    private readonly IRestrictedZonePolicyRepository _repository;
    private readonly ILogger<ActivateRestrictedZonePolicyCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ActivateRestrictedZonePolicyCommandHandler(IRestrictedZonePolicyRepository repository, ILogger<ActivateRestrictedZonePolicyCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RestrictedZonePolicyActionResult>> Handle(ActivateRestrictedZonePolicyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = RestrictedZonePolicyId.FromGuid(request.RestrictedZonePolicyId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<RestrictedZonePolicyActionResult>.Failure(new ApplicationError(
                    "PhysicalAccessControl.RestrictedZonePolicy.NotFound",
                    "The requested RestrictedZonePolicy record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Activate(
                UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<RestrictedZonePolicyActionResult>.Success(new RestrictedZonePolicyActionResult(
                aggregate.RestrictedZonePolicyId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute Activate for RestrictedZonePolicy {RestrictedZonePolicyId}", request.RestrictedZonePolicyId);

            return Result<RestrictedZonePolicyActionResult>.Failure(new ApplicationError(
                "PhysicalAccessControl.RestrictedZonePolicy.ActivateFailed",
                "Unable to activate the RestrictedZonePolicy record at this time.",
                Guid.NewGuid()));
        }
    }
}

// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for DefineRestrictedZonePolicyCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.Aggregates;
using PlantSafety.Domain.PhysicalAccessControl.RestrictedZonePolicies.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PhysicalAccessControl.Commands;

/// <summary>Handles <see cref="DefineRestrictedZonePolicyCommand"/>.</summary>
public sealed class DefineRestrictedZonePolicyCommandHandler : IRequestHandler<DefineRestrictedZonePolicyCommand, Result<RestrictedZonePolicyActionResult>>
{
    private readonly IRestrictedZonePolicyRepository _repository;
    private readonly ILogger<DefineRestrictedZonePolicyCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public DefineRestrictedZonePolicyCommandHandler(IRestrictedZonePolicyRepository repository, ILogger<DefineRestrictedZonePolicyCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RestrictedZonePolicyActionResult>> Handle(DefineRestrictedZonePolicyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            var aggregate = RestrictedZonePolicy.Create(
                performedBy,
                UtcTimestamp.Now(),
                new CorrelationId(request.CorrelationId),
                new CausationId(request.CausationId),
                PerformedByActor.User(performedBy),
                request.TenantId,
                request.SiteId,
                request.SafetyZoneId,
                request.PolicyName,
                request.AllowedAccessLevel,
                request.EnforceAfterHoursLockout);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<RestrictedZonePolicyActionResult>.Success(new RestrictedZonePolicyActionResult(
                aggregate.RestrictedZonePolicyId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to define RestrictedZonePolicy for Tenant {TenantId}", request.TenantId);

            return Result<RestrictedZonePolicyActionResult>.Failure(new ApplicationError(
                "PhysicalAccessControl.RestrictedZonePolicy.DefineFailed",
                "Unable to define the RestrictedZonePolicy record at this time.",
                Guid.NewGuid()));
        }
    }
}

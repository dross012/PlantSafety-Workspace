// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for RegisterSafetyZoneCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.PhysicalAccessControl.SafetyZones.Aggregates;
using PlantSafety.Domain.PhysicalAccessControl.SafetyZones.Enums;
using PlantSafety.Domain.PhysicalAccessControl.SafetyZones.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PhysicalAccessControl.Commands;

/// <summary>Handles <see cref="RegisterSafetyZoneCommand"/>.</summary>
public sealed class RegisterSafetyZoneCommandHandler : IRequestHandler<RegisterSafetyZoneCommand, Result<SafetyZoneActionResult>>
{
    private readonly ISafetyZoneRepository _repository;
    private readonly ILogger<RegisterSafetyZoneCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RegisterSafetyZoneCommandHandler(ISafetyZoneRepository repository, ILogger<RegisterSafetyZoneCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<SafetyZoneActionResult>> Handle(RegisterSafetyZoneCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var performedBy = UserId.FromGuid(request.PerformedByUserId);
            var hazardLevel = Enum.TryParse<SafetyZoneHazardLevel>(request.HazardLevel, true, out var parsed)
                ? parsed
                : SafetyZoneHazardLevel.Low;

            var aggregate = SafetyZone.Create(
                performedBy,
                UtcTimestamp.Now(),
                new CorrelationId(request.CorrelationId),
                new CausationId(request.CausationId),
                PerformedByActor.User(performedBy),
                request.TenantId,
                request.SiteId,
                request.ZoneCode,
                request.ZoneName,
                hazardLevel,
                request.MaxOccupancy);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<SafetyZoneActionResult>.Success(new SafetyZoneActionResult(
                aggregate.SafetyZoneId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register SafetyZone for Tenant {TenantId}", request.TenantId);

            return Result<SafetyZoneActionResult>.Failure(new ApplicationError(
                "PhysicalAccessControl.SafetyZone.RegisterFailed",
                "Unable to register the SafetyZone record at this time.",
                Guid.NewGuid()));
        }
    }
}

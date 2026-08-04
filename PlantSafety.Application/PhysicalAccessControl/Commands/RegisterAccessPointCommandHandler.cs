// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for RegisterAccessPointCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Aggregates;
using PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Enums;
using PlantSafety.Domain.PhysicalAccessControl.AccessPoints.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PhysicalAccessControl.Commands;

/// <summary>Handles <see cref="RegisterAccessPointCommand"/>.</summary>
public sealed class RegisterAccessPointCommandHandler : IRequestHandler<RegisterAccessPointCommand, Result<AccessPointActionResult>>
{
    private readonly IAccessPointRepository _repository;
    private readonly ILogger<RegisterAccessPointCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RegisterAccessPointCommandHandler(IAccessPointRepository repository, ILogger<RegisterAccessPointCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AccessPointActionResult>> Handle(RegisterAccessPointCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var performedBy = UserId.FromGuid(request.PerformedByUserId);
            var deviceType = Enum.TryParse<AccessPointDeviceType>(request.DeviceType, true, out var parsed)
                ? parsed
                : AccessPointDeviceType.BadgeReader;

            var aggregate = AccessPoint.Create(
                performedBy,
                UtcTimestamp.Now(),
                new CorrelationId(request.CorrelationId),
                new CausationId(request.CausationId),
                PerformedByActor.User(performedBy),
                request.TenantId,
                request.SiteId,
                request.AccessPointCode,
                request.Description,
                deviceType,
                request.SafetyZoneId,
                request.IsPerimeter);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<AccessPointActionResult>.Success(new AccessPointActionResult(
                aggregate.AccessPointId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register AccessPoint for Tenant {TenantId}", request.TenantId);

            return Result<AccessPointActionResult>.Failure(new ApplicationError(
                "PhysicalAccessControl.AccessPoint.RegisterFailed",
                "Unable to register the AccessPoint record at this time.",
                Guid.NewGuid()));
        }
    }
}

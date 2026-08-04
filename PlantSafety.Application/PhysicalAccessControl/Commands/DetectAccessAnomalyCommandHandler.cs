// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for DetectAccessAnomalyCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Aggregates;
using PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Enums;
using PlantSafety.Domain.PhysicalAccessControl.AccessAnomalies.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PhysicalAccessControl.Commands;

/// <summary>Handles <see cref="DetectAccessAnomalyCommand"/>.</summary>
public sealed class DetectAccessAnomalyCommandHandler : IRequestHandler<DetectAccessAnomalyCommand, Result<AccessAnomalyActionResult>>
{
    private readonly IAccessAnomalyRepository _repository;
    private readonly ILogger<DetectAccessAnomalyCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public DetectAccessAnomalyCommandHandler(IAccessAnomalyRepository repository, ILogger<DetectAccessAnomalyCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AccessAnomalyActionResult>> Handle(DetectAccessAnomalyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var performedBy = UserId.FromGuid(request.PerformedByUserId);
            var anomalyType = Enum.TryParse<AccessAnomalyType>(request.AnomalyType, true, out var parsed)
                ? parsed
                : AccessAnomalyType.UnauthorizedAccess;

            var aggregate = AccessAnomaly.Create(
                performedBy,
                UtcTimestamp.Now(),
                new CorrelationId(request.CorrelationId),
                new CausationId(request.CausationId),
                PerformedByActor.User(performedBy),
                request.TenantId,
                request.SiteId,
                request.AccessEventId,
                request.AccessPointId,
                request.CredentialId,
                request.SafetyZoneId,
                anomalyType,
                request.SeverityScore);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<AccessAnomalyActionResult>.Success(new AccessAnomalyActionResult(
                aggregate.AccessAnomalyId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record detected AccessAnomaly for Tenant {TenantId}", request.TenantId);

            return Result<AccessAnomalyActionResult>.Failure(new ApplicationError(
                "PhysicalAccessControl.AccessAnomaly.DetectFailed",
                "Unable to record the AccessAnomaly at this time.",
                Guid.NewGuid()));
        }
    }
}

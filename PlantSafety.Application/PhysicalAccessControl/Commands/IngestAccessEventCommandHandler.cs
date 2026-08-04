// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for IngestAccessEventCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Aggregates;
using PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Enums;
using PlantSafety.Domain.PhysicalAccessControl.AccessEvents.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PhysicalAccessControl.Commands;

/// <summary>Handles <see cref="IngestAccessEventCommand"/>.</summary>
public sealed class IngestAccessEventCommandHandler : IRequestHandler<IngestAccessEventCommand, Result<AccessEventActionResult>>
{
    private readonly IAccessEventRepository _repository;
    private readonly ILogger<IngestAccessEventCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public IngestAccessEventCommandHandler(IAccessEventRepository repository, ILogger<IngestAccessEventCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AccessEventActionResult>> Handle(IngestAccessEventCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var performedBy = UserId.FromGuid(request.PerformedByUserId);
            var eventType = Enum.TryParse<AccessEventType>(request.EventType, true, out var parsedType)
                ? parsedType
                : AccessEventType.Granted;
            var direction = Enum.TryParse<AccessEventDirection>(request.Direction, true, out var parsedDirection)
                ? parsedDirection
                : AccessEventDirection.In;

            var aggregate = AccessEvent.Create(
                performedBy,
                UtcTimestamp.Now(),
                new CorrelationId(request.CorrelationId),
                new CausationId(request.CausationId),
                PerformedByActor.User(performedBy),
                request.TenantId,
                request.SiteId,
                request.AccessPointId,
                request.CredentialId,
                request.SafetyZoneId,
                eventType,
                direction,
                UtcTimestamp.From(request.OccurredAtUtc));

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<AccessEventActionResult>.Success(new AccessEventActionResult(
                aggregate.AccessEventId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to ingest AccessEvent for Tenant {TenantId}", request.TenantId);

            return Result<AccessEventActionResult>.Failure(new ApplicationError(
                "PhysicalAccessControl.AccessEvent.IngestFailed",
                "Unable to ingest the AccessEvent record at this time.",
                Guid.NewGuid()));
        }
    }
}

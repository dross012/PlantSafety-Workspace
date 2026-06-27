// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for CreateCrisisEventCommand.
//   This is a high-privilege operation gated by SafetyEvents.CrisisManagement.Declare.
//   The outbox processor dispatches CrisisDeclaredEvent to emergency notification consumers.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Clock;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Utilities;
using PlantSafety.Application.Features.SafetyEvents.Commands.CrisisManagement.Create;

namespace PlantSafety.Application.Features.SafetyEvents.Handlers.CrisisManagement;

/// <summary>Handles <see cref="CreateCrisisEventCommand"/>. Returns the new CrisisEventId.</summary>
public sealed class CreateCrisisEventCommandHandler : ICommandHandler<CreateCrisisEventCommand, Guid>
{
    private readonly ICrisisEventRepository _repository;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<CreateCrisisEventCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateCrisisEventCommandHandler(
        ICrisisEventRepository repository,
        IDateTimeProvider clock,
        ILogger<CreateCrisisEventCommandHandler> logger)
    {
        _repository = repository;
        _clock = clock;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Guid> Handle(CreateCrisisEventCommand request, CancellationToken cancellationToken)
    {
        var crisisEventId = CorrelationIdGenerator.NewId(_clock.UtcNowOffset);

        await _repository.CreateAsync(
            crisisEventId: crisisEventId,
            tenantId: request.TenantId,
            siteId: request.SiteId,
            title: request.Title,
            description: request.Description,
            crisisCategory: request.CrisisCategory,
            incidentCommanderId: request.IncidentCommanderId,
            actorId: request.ActorId,
            correlationId: request.CorrelationId,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "CrisisEvent {CrisisEventId} declared for Tenant {TenantId} Site {SiteId} by Actor {ActorId} CorrelationId {CorrelationId}",
            crisisEventId, request.TenantId, request.SiteId, request.ActorId, request.CorrelationId);

        return crisisEventId;
    }
}

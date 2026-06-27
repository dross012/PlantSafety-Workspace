// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for CreateCorrectiveActionCommand.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Clock;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Utilities;
using PlantSafety.Application.Features.ContinuousImprovement.Commands.CAPA.Create;

namespace PlantSafety.Application.Features.ContinuousImprovement.Handlers.CAPA;

/// <summary>Handles <see cref="CreateCorrectiveActionCommand"/>. Returns the new CaId.</summary>
public sealed class CreateCorrectiveActionCommandHandler : ICommandHandler<CreateCorrectiveActionCommand, Guid>
{
    private readonly ICorrectiveActionRepository _repository;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<CreateCorrectiveActionCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateCorrectiveActionCommandHandler(
        ICorrectiveActionRepository repository,
        IDateTimeProvider clock,
        ILogger<CreateCorrectiveActionCommandHandler> logger)
    {
        _repository = repository;
        _clock = clock;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Guid> Handle(CreateCorrectiveActionCommand request, CancellationToken cancellationToken)
    {
        var caId = CorrelationIdGenerator.NewId(_clock.UtcNowOffset);

        await _repository.CreateAsync(
            caId: caId,
            tenantId: request.TenantId,
            sourceEventId: request.SourceEventId,
            sourceEventType: request.SourceEventType,
            title: request.Title,
            description: request.Description,
            actionType: request.ActionType,
            priority: request.Priority,
            assignedToWorkerId: request.AssignedToWorkerId,
            dueAtUtc: request.DueAtUtc,
            actorId: request.ActorId,
            correlationId: request.CorrelationId,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "CorrectiveAction {CaId} created for SourceEvent {SourceEventId} Tenant {TenantId} by Actor {ActorId}",
            caId, request.SourceEventId, request.TenantId, request.ActorId);

        return caId;
    }
}

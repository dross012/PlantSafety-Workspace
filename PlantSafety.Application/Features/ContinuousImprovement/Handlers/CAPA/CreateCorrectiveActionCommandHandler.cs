// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// UPDATED: 2026-08-06 -- now constructs the real CorrectiveAction aggregate via its domain
// factory (CorrectiveAction.Create) instead of pushing primitives straight to Dapper/SQL, so
// CorrectiveActionCreatedEvent is actually raised and can reach Shared.EventBus.
// FILE DESCRIPTION: Handler for CreateCorrectiveActionCommand.
// SchemaVersion: 2

using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Features.ContinuousImprovement.Commands.CAPA.Create;
using PlantSafety.Domain.ContinuousImprovement.CAPA.Aggregates;

namespace PlantSafety.Application.Features.ContinuousImprovement.Handlers.CAPA;

/// <summary>Handles <see cref="CreateCorrectiveActionCommand"/>. Returns the new CaId.</summary>
public sealed class CreateCorrectiveActionCommandHandler : ICommandHandler<CreateCorrectiveActionCommand, Guid>
{
    private readonly ICorrectiveActionRepository _repository;
    private readonly ILogger<CreateCorrectiveActionCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateCorrectiveActionCommandHandler(
        ICorrectiveActionRepository repository,
        ILogger<CreateCorrectiveActionCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Guid> Handle(CreateCorrectiveActionCommand request, CancellationToken cancellationToken)
    {
        var actorId = UserId.FromGuid(request.ActorId);

        var aggregate = CorrectiveAction.Create(
            createdBy: actorId,
            createdAtUtc: UtcTimestamp.From(request.RequestedAtUtc),
            correlationId: new CorrelationId(request.CorrelationId),
            causationId: new CausationId(request.CausationId ?? request.CommandId),
            actor: PerformedByActor.User(actorId),
            tenantId: request.TenantId,
            sourceEventId: request.SourceEventId,
            sourceEventType: request.SourceEventType,
            title: request.Title,
            description: request.Description,
            actionType: request.ActionType,
            priority: request.Priority,
            assignedToWorkerId: UserId.FromGuid(request.AssignedToWorkerId),
            dueAtUtc: UtcTimestamp.From(request.DueAtUtc.UtcDateTime));

        await _repository.AddAsync(aggregate, request.ActorId, request.CorrelationId, cancellationToken);

        _logger.LogInformation(
            "CorrectiveAction {CaId} created for SourceEvent {SourceEventId} Tenant {TenantId} by Actor {ActorId}",
            aggregate.CorrectiveActionId.Value, request.SourceEventId, request.TenantId, request.ActorId);

        return aggregate.CorrectiveActionId.Value;
    }
}

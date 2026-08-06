// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-06
// FILE DESCRIPTION: Handler for CloseCorrectiveActionCommand. Loads the CorrectiveAction
// aggregate, applies Close (lifecycle invariants), persists via ICorrectiveActionRepository.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Features.ContinuousImprovement.Commands.CAPA.Close;
using PlantSafety.Domain.ContinuousImprovement.CAPA.Exceptions;
using PlantSafety.Domain.ContinuousImprovement.CAPA.ValueObjects.Ids;

namespace PlantSafety.Application.Features.ContinuousImprovement.Handlers.CAPA;

/// <summary>Handles <see cref="CloseCorrectiveActionCommand"/>.</summary>
public sealed class CloseCorrectiveActionCommandHandler : ICommandHandler<CloseCorrectiveActionCommand, Guid>
{
    private readonly ICorrectiveActionRepository _repository;
    private readonly ILogger<CloseCorrectiveActionCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CloseCorrectiveActionCommandHandler(
        ICorrectiveActionRepository repository,
        ILogger<CloseCorrectiveActionCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Guid> Handle(CloseCorrectiveActionCommand request, CancellationToken cancellationToken)
    {
        var correlationId = new CorrelationId(request.CorrelationId);
        var causationId = new CausationId(request.CausationId ?? request.CommandId);
        var caId = CorrectiveActionId.FromGuid(request.CaId);

        var aggregate = await _repository.GetAggregateByIdAsync(request.TenantId, request.CaId, cancellationToken)
            ?? throw new CorrectiveActionNotFoundException(caId, correlationId, causationId);

        aggregate.Close(
            closedAtUtc: UtcTimestamp.Now(),
            actor: PerformedByActor.User(UserId.FromGuid(request.ActorId)),
            correlationId: correlationId,
            causationId: causationId);

        await _repository.UpdateAsync(aggregate, request.ActorId, request.CorrelationId, cancellationToken);

        _logger.LogInformation(
            "CorrectiveAction {CaId} closed for Tenant {TenantId} by Actor {ActorId}",
            request.CaId, request.TenantId, request.ActorId);

        return request.CaId;
    }
}

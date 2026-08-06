// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-06
// FILE DESCRIPTION: Handler for StartCorrectiveActionCommand. Loads the CorrectiveAction
// aggregate, applies Start (lifecycle invariants), persists via ICorrectiveActionRepository.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Features.ContinuousImprovement.Commands.CAPA.Start;
using PlantSafety.Domain.ContinuousImprovement.CAPA.Exceptions;
using PlantSafety.Domain.ContinuousImprovement.CAPA.ValueObjects.Ids;

namespace PlantSafety.Application.Features.ContinuousImprovement.Handlers.CAPA;

/// <summary>Handles <see cref="StartCorrectiveActionCommand"/>.</summary>
public sealed class StartCorrectiveActionCommandHandler : ICommandHandler<StartCorrectiveActionCommand, Guid>
{
    private readonly ICorrectiveActionRepository _repository;
    private readonly ILogger<StartCorrectiveActionCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public StartCorrectiveActionCommandHandler(
        ICorrectiveActionRepository repository,
        ILogger<StartCorrectiveActionCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Guid> Handle(StartCorrectiveActionCommand request, CancellationToken cancellationToken)
    {
        var correlationId = new CorrelationId(request.CorrelationId);
        var causationId = new CausationId(request.CausationId ?? request.CommandId);
        var caId = CorrectiveActionId.FromGuid(request.CaId);

        var aggregate = await _repository.GetAggregateByIdAsync(request.TenantId, request.CaId, cancellationToken)
            ?? throw new CorrectiveActionNotFoundException(caId, correlationId, causationId);

        aggregate.Start(
            nowUtc: UtcTimestamp.Now(),
            actor: PerformedByActor.User(UserId.FromGuid(request.ActorId)),
            correlationId: correlationId,
            causationId: causationId);

        await _repository.UpdateAsync(aggregate, request.ActorId, request.CorrelationId, cancellationToken);

        _logger.LogInformation(
            "CorrectiveAction {CaId} started for Tenant {TenantId} by Actor {ActorId}",
            request.CaId, request.TenantId, request.ActorId);

        return request.CaId;
    }
}

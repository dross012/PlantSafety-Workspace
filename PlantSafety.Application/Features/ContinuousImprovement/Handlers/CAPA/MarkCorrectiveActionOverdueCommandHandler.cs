// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-06
// FILE DESCRIPTION: Handler for MarkCorrectiveActionOverdueCommand. Loads the CorrectiveAction
// aggregate, applies MarkOverdue (lifecycle invariants; no-op if not Open/InProgress), persists
// via ICorrectiveActionRepository only when the aggregate's state actually changed.
// SchemaVersion: 1

using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Features.ContinuousImprovement.Commands.CAPA.MarkOverdue;
using PlantSafety.Domain.ContinuousImprovement.CAPA.Exceptions;
using PlantSafety.Domain.ContinuousImprovement.CAPA.ValueObjects.Ids;

namespace PlantSafety.Application.Features.ContinuousImprovement.Handlers.CAPA;

/// <summary>Handles <see cref="MarkCorrectiveActionOverdueCommand"/>.</summary>
public sealed class MarkCorrectiveActionOverdueCommandHandler : ICommandHandler<MarkCorrectiveActionOverdueCommand, Guid>
{
    private readonly ICorrectiveActionRepository _repository;
    private readonly ILogger<MarkCorrectiveActionOverdueCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public MarkCorrectiveActionOverdueCommandHandler(
        ICorrectiveActionRepository repository,
        ILogger<MarkCorrectiveActionOverdueCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Guid> Handle(MarkCorrectiveActionOverdueCommand request, CancellationToken cancellationToken)
    {
        var correlationId = new CorrelationId(request.CorrelationId);
        var causationId = new CausationId(request.CausationId ?? request.CommandId);
        var caId = CorrectiveActionId.FromGuid(request.CaId);

        var aggregate = await _repository.GetAggregateByIdAsync(request.TenantId, request.CaId, cancellationToken)
            ?? throw new CorrectiveActionNotFoundException(caId, correlationId, causationId);

        aggregate.MarkOverdue(
            nowUtc: UtcTimestamp.Now(),
            actor: PerformedByActor.User(UserId.FromGuid(request.ActorId)),
            correlationId: correlationId,
            causationId: causationId);

        // MarkOverdue is a documented no-op when the action isn't Open/InProgress (e.g. already
        // Closed/AwaitingVerification) -- no domain event means no state actually changed, so
        // skip the write entirely rather than re-issuing an update for nothing.
        if (aggregate.DomainEvents.Count > 0)
        {
            await _repository.UpdateAsync(aggregate, request.ActorId, request.CorrelationId, cancellationToken);

            _logger.LogInformation(
                "CorrectiveAction {CaId} marked Overdue for Tenant {TenantId} by Actor {ActorId}",
                request.CaId, request.TenantId, request.ActorId);
        }
        else
        {
            _logger.LogDebug(
                "CorrectiveAction {CaId} MarkOverdue was a no-op (status {Status} not eligible) for Tenant {TenantId}",
                request.CaId, aggregate.Status, request.TenantId);
        }

        return request.CaId;
    }
}

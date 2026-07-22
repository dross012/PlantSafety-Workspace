// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for TriggerAuditFindingCapaCommand. Pure orchestration: loads the AuditFinding aggregate, invokes its TriggerCapa() lifecycle method, and persists the change through the domain repository interface. No business rules live here.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Auditing.AuditFinding.Interfaces;
using PlantSafety.Domain.Auditing.AuditFinding.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Auditing.Commands;

/// <summary>Handles <see cref="TriggerAuditFindingCapaCommand"/>.</summary>
public sealed class TriggerAuditFindingCapaCommandHandler : IRequestHandler<TriggerAuditFindingCapaCommand, Result<TriggerAuditFindingCapaResult>>
{
    private readonly IAuditFindingRepository _repository;
    private readonly ILogger<TriggerAuditFindingCapaCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public TriggerAuditFindingCapaCommandHandler(IAuditFindingRepository repository, ILogger<TriggerAuditFindingCapaCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<TriggerAuditFindingCapaResult>> Handle(TriggerAuditFindingCapaCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = AuditFindingId.FromGuid(request.AuditFindingId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<TriggerAuditFindingCapaResult>.Failure(new ApplicationError(
                    "Auditing.AuditFinding.NotFound",
                    "The requested AuditFinding record was not found.",
                    request.CorrelationId,
                    ErrorSeverity.NotFound));
            }

            var ownerId = UserId.FromGuid(request.OwnerId);
            var assignedToUserId = UserId.FromGuid(request.AssignedToUserId);

            aggregate.TriggerCapa(
                assignedToUserId: assignedToUserId,
                dueDateUtc: UtcTimestamp.From(request.DueDateUtc),
                actor: PerformedByActor.User(ownerId),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                occurredAtUtc: UtcTimestamp.Now());

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<TriggerAuditFindingCapaResult>.Success(new TriggerAuditFindingCapaResult(
                aggregate.AuditFindingId.Value,
                aggregate.Status.ToString(),
                aggregate.DueDateUtc!.Value.Value));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to trigger CAPA for AuditFinding {AuditFindingId}", request.AuditFindingId);

            return Result<TriggerAuditFindingCapaResult>.Failure(new ApplicationError(
                "Auditing.AuditFinding.TriggerCapaFailed",
                "Unable to trigger the corrective action at this time.",
                request.CorrelationId));
        }
    }
}

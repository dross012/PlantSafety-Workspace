// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for CompleteChecklistCompletionCommand. Pure orchestration: loads the ChecklistCompletion aggregate, invokes its Complete() lifecycle method, and persists the change through the domain repository interface. No business rules live here.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Checklists.ChecklistCompletion.Interfaces;
using PlantSafety.Domain.Checklists.ChecklistCompletion.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Checklists.Commands;

/// <summary>Handles <see cref="CompleteChecklistCompletionCommand"/>.</summary>
public sealed class CompleteChecklistCompletionCommandHandler : IRequestHandler<CompleteChecklistCompletionCommand, Result<CompleteChecklistCompletionResult>>
{
    private readonly IChecklistCompletionRepository _repository;
    private readonly ILogger<CompleteChecklistCompletionCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CompleteChecklistCompletionCommandHandler(IChecklistCompletionRepository repository, ILogger<CompleteChecklistCompletionCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CompleteChecklistCompletionResult>> Handle(CompleteChecklistCompletionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = ChecklistCompletionId.FromGuid(request.ChecklistCompletionId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<CompleteChecklistCompletionResult>.Failure(new ApplicationError(
                    "Checklists.ChecklistCompletion.NotFound",
                    "The requested ChecklistCompletion record was not found.",
                    request.CorrelationId,
                    ErrorSeverity.NotFound));
            }

            var ownerId = UserId.FromGuid(request.OwnerId);

            aggregate.Complete(
                actor: PerformedByActor.User(ownerId),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                occurredAtUtc: UtcTimestamp.Now());

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<CompleteChecklistCompletionResult>.Success(new CompleteChecklistCompletionResult(
                aggregate.ChecklistCompletionId.Value,
                aggregate.Status.ToString(),
                aggregate.Outcome,
                aggregate.ItemsTotal,
                aggregate.ItemsPassed,
                aggregate.ItemsFailed));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to complete ChecklistCompletion {ChecklistCompletionId}", request.ChecklistCompletionId);

            return Result<CompleteChecklistCompletionResult>.Failure(new ApplicationError(
                "Checklists.ChecklistCompletion.CompleteFailed",
                "Unable to complete the ChecklistCompletion at this time.",
                request.CorrelationId));
        }
    }
}

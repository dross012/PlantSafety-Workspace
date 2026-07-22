// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for RecordChecklistItemResultCommand. Pure orchestration: loads the ChecklistCompletion aggregate, invokes its RecordItemResult() lifecycle method, and persists the change through the domain repository interface. No business rules live here.
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

/// <summary>Handles <see cref="RecordChecklistItemResultCommand"/>.</summary>
public sealed class RecordChecklistItemResultCommandHandler : IRequestHandler<RecordChecklistItemResultCommand, Result<RecordChecklistItemResultResult>>
{
    private readonly IChecklistCompletionRepository _repository;
    private readonly ILogger<RecordChecklistItemResultCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordChecklistItemResultCommandHandler(IChecklistCompletionRepository repository, ILogger<RecordChecklistItemResultCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RecordChecklistItemResultResult>> Handle(RecordChecklistItemResultCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = ChecklistCompletionId.FromGuid(request.ChecklistCompletionId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<RecordChecklistItemResultResult>.Failure(new ApplicationError(
                    "Checklists.ChecklistCompletion.NotFound",
                    "The requested ChecklistCompletion record was not found.",
                    request.CorrelationId,
                    ErrorSeverity.NotFound));
            }

            var ownerId = UserId.FromGuid(request.OwnerId);

            var result = aggregate.RecordItemResult(
                checklistTemplateItemId: request.ChecklistTemplateItemId,
                passed: request.Passed,
                notes: request.Notes,
                actor: PerformedByActor.User(ownerId),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                occurredAtUtc: UtcTimestamp.Now());

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<RecordChecklistItemResultResult>.Success(new RecordChecklistItemResultResult(
                aggregate.ChecklistCompletionId.Value,
                result.ItemResultId,
                aggregate.ItemsPassed,
                aggregate.ItemsFailed));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record item result for ChecklistCompletion {ChecklistCompletionId}", request.ChecklistCompletionId);

            return Result<RecordChecklistItemResultResult>.Failure(new ApplicationError(
                "Checklists.ChecklistCompletion.RecordItemResultFailed",
                "Unable to record the checklist item result at this time.",
                request.CorrelationId));
        }
    }
}

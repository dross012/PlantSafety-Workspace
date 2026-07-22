// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for FlagFailedChecklistItemCommand. Pure orchestration: loads the ChecklistCompletion aggregate, invokes its FlagFailedItem() lifecycle method, and persists the change through the domain repository interface. No business rules live here.
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

/// <summary>Handles <see cref="FlagFailedChecklistItemCommand"/>.</summary>
public sealed class FlagFailedChecklistItemCommandHandler : IRequestHandler<FlagFailedChecklistItemCommand, Result<FlagFailedChecklistItemResult>>
{
    private readonly IChecklistCompletionRepository _repository;
    private readonly ILogger<FlagFailedChecklistItemCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public FlagFailedChecklistItemCommandHandler(IChecklistCompletionRepository repository, ILogger<FlagFailedChecklistItemCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<FlagFailedChecklistItemResult>> Handle(FlagFailedChecklistItemCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = ChecklistCompletionId.FromGuid(request.ChecklistCompletionId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<FlagFailedChecklistItemResult>.Failure(new ApplicationError(
                    "Checklists.ChecklistCompletion.NotFound",
                    "The requested ChecklistCompletion record was not found.",
                    request.CorrelationId,
                    ErrorSeverity.NotFound));
            }

            var ownerId = UserId.FromGuid(request.OwnerId);

            var result = aggregate.FlagFailedItem(
                checklistTemplateItemId: request.ChecklistTemplateItemId,
                description: request.Description,
                actor: PerformedByActor.User(ownerId),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                occurredAtUtc: UtcTimestamp.Now());

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<FlagFailedChecklistItemResult>.Success(new FlagFailedChecklistItemResult(
                aggregate.ChecklistCompletionId.Value,
                result.ItemResultId,
                aggregate.CriticalFailureCount));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to flag critical item for ChecklistCompletion {ChecklistCompletionId}", request.ChecklistCompletionId);

            return Result<FlagFailedChecklistItemResult>.Failure(new ApplicationError(
                "Checklists.ChecklistCompletion.FlagFailedItemFailed",
                "Unable to flag the critical checklist item at this time.",
                request.CorrelationId));
        }
    }
}

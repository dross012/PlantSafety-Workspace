// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for StartChecklistCompletionCommand. Pure orchestration: loads the ChecklistCompletion aggregate, invokes its Start() lifecycle method, and persists the change through the domain repository interface. No business rules live here.
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

/// <summary>Handles <see cref="StartChecklistCompletionCommand"/>.</summary>
public sealed class StartChecklistCompletionCommandHandler : IRequestHandler<StartChecklistCompletionCommand, Result<StartChecklistCompletionResult>>
{
    private readonly IChecklistCompletionRepository _repository;
    private readonly ILogger<StartChecklistCompletionCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public StartChecklistCompletionCommandHandler(IChecklistCompletionRepository repository, ILogger<StartChecklistCompletionCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<StartChecklistCompletionResult>> Handle(StartChecklistCompletionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = ChecklistCompletionId.FromGuid(request.ChecklistCompletionId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<StartChecklistCompletionResult>.Failure(new ApplicationError(
                    "Checklists.ChecklistCompletion.NotFound",
                    "The requested ChecklistCompletion record was not found.",
                    request.CorrelationId,
                    ErrorSeverity.NotFound));
            }

            var ownerId = UserId.FromGuid(request.OwnerId);

            aggregate.Start(
                actor: PerformedByActor.User(ownerId),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                occurredAtUtc: UtcTimestamp.Now());

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<StartChecklistCompletionResult>.Success(new StartChecklistCompletionResult(
                aggregate.ChecklistCompletionId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start ChecklistCompletion {ChecklistCompletionId}", request.ChecklistCompletionId);

            return Result<StartChecklistCompletionResult>.Failure(new ApplicationError(
                "Checklists.ChecklistCompletion.StartFailed",
                "Unable to start the ChecklistCompletion at this time.",
                request.CorrelationId));
        }
    }
}

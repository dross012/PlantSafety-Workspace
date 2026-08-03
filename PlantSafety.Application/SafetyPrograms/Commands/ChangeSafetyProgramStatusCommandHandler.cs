// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Handler for ChangeSafetyProgramStatusCommand. Loads the aggregate, invokes
//   its ChangeStatus lifecycle method (which enforces valid transitions), and persists the
//   update. No business rules live here.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.SafetyPrograms.SafetyProgram.Enums;
using PlantSafety.Domain.SafetyPrograms.SafetyProgram.Exceptions;
using PlantSafety.Domain.SafetyPrograms.SafetyProgram.Interfaces;
using PlantSafety.Domain.SafetyPrograms.SafetyProgram.ValueObjects.Ids;

using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyPrograms.Commands;

/// <summary>Handles <see cref="ChangeSafetyProgramStatusCommand"/>.</summary>
public sealed class ChangeSafetyProgramStatusCommandHandler : IRequestHandler<ChangeSafetyProgramStatusCommand, Result<ChangeSafetyProgramStatusResult>>
{
    private readonly ISafetyProgramRepository _repository;
    private readonly ILogger<ChangeSafetyProgramStatusCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ChangeSafetyProgramStatusCommandHandler(ISafetyProgramRepository repository, ILogger<ChangeSafetyProgramStatusCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ChangeSafetyProgramStatusResult>> Handle(ChangeSafetyProgramStatusCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!Enum.TryParse<SafetyProgramStatus>(request.NewStatus, ignoreCase: true, out var newStatus))
            {
                return Result<ChangeSafetyProgramStatusResult>.Failure(new ApplicationError(
                    "SafetyPrograms.SafetyProgram.InvalidStatus",
                    $"Status '{request.NewStatus}' is not a recognized SafetyProgram status.",
                    request.CorrelationId,
                    ErrorSeverity.Validation));
            }

            var id = SafetyProgramId.FromGuid(request.SafetyProgramId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);

            if (aggregate is null)
            {
                return Result<ChangeSafetyProgramStatusResult>.Failure(new ApplicationError(
                    "SafetyPrograms.SafetyProgram.NotFound",
                    $"SafetyProgram '{request.SafetyProgramId}' was not found.",
                    request.CorrelationId,
                    ErrorSeverity.NotFound));
            }

            var actorId = UserId.FromGuid(request.PerformedByUserId);

            aggregate.ChangeStatus(
                newStatus,
                PerformedByActor.User(actorId),
                new CorrelationId(request.CorrelationId),
                new CausationId(request.CausationId),
                UtcTimestamp.Now());

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<ChangeSafetyProgramStatusResult>.Success(new ChangeSafetyProgramStatusResult(
                aggregate.SafetyProgramId.Value,
                aggregate.Status.ToString()));
        }
        catch (SafetyProgramInvalidStatusTransitionException ex)
        {
            return Result<ChangeSafetyProgramStatusResult>.Failure(new ApplicationError(
                ex.ErrorCode,
                ex.Message,
                request.CorrelationId,
                ErrorSeverity.Domain));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to change SafetyProgram status for {SafetyProgramId}", request.SafetyProgramId);

            return Result<ChangeSafetyProgramStatusResult>.Failure(new ApplicationError(
                "SafetyPrograms.SafetyProgram.ChangeStatusFailed",
                "Unable to change the SafetyProgram status at this time.",
                request.CorrelationId));
        }
    }
}

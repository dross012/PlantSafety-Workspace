// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for FlagPPEAssignmentForReplacementCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.PPE.PPEAssignment.Interfaces;
using PlantSafety.Domain.PPE.PPEAssignment.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PPE.Commands;

/// <summary>Handles <see cref="FlagPPEAssignmentForReplacementCommand"/>.</summary>
public sealed class FlagPPEAssignmentForReplacementCommandHandler : IRequestHandler<FlagPPEAssignmentForReplacementCommand, Result<PPEAssignmentActionResult>>
{
    private readonly IPPEAssignmentRepository _repository;
    private readonly ILogger<FlagPPEAssignmentForReplacementCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public FlagPPEAssignmentForReplacementCommandHandler(IPPEAssignmentRepository repository, ILogger<FlagPPEAssignmentForReplacementCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PPEAssignmentActionResult>> Handle(FlagPPEAssignmentForReplacementCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = PPEAssignmentId.FromGuid(request.PPEAssignmentId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<PPEAssignmentActionResult>.Failure(new ApplicationError(
                    "PPE.PPEAssignment.NotFound",
                    "The requested PPE assignment could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.FlagForReplacement(
                reason: request.Reason,
                flaggedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<PPEAssignmentActionResult>.Success(new PPEAssignmentActionResult(
                aggregate.PPEAssignmentId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to flag PPEAssignment {PPEAssignmentId} for replacement", request.PPEAssignmentId);

            return Result<PPEAssignmentActionResult>.Failure(new ApplicationError(
                "PPE.PPEAssignment.FlagForReplacementFailed",
                "Unable to flag the PPE assignment for replacement at this time.",
                Guid.NewGuid()));
        }
    }
}

// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for RecordPPEFitTestCommand.
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

/// <summary>Handles <see cref="RecordPPEFitTestCommand"/>.</summary>
public sealed class RecordPPEFitTestCommandHandler : IRequestHandler<RecordPPEFitTestCommand, Result<PPEAssignmentActionResult>>
{
    private readonly IPPEAssignmentRepository _repository;
    private readonly ILogger<RecordPPEFitTestCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordPPEFitTestCommandHandler(IPPEAssignmentRepository repository, ILogger<RecordPPEFitTestCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PPEAssignmentActionResult>> Handle(RecordPPEFitTestCommand request, CancellationToken cancellationToken)
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

            aggregate.RecordFitTest(
                passed: request.Passed,
                notes: request.Notes,
                testedAtUtc: UtcTimestamp.Now(),
                fitTestExpiresAtUtc: request.FitTestExpiresAtUtc.HasValue ? UtcTimestamp.From(request.FitTestExpiresAtUtc.Value) : null,
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
            _logger.LogError(ex, "Failed to record fit test for PPEAssignment {PPEAssignmentId}", request.PPEAssignmentId);

            return Result<PPEAssignmentActionResult>.Failure(new ApplicationError(
                "PPE.PPEAssignment.RecordFitTestFailed",
                "Unable to record the fit test result at this time.",
                Guid.NewGuid()));
        }
    }
}

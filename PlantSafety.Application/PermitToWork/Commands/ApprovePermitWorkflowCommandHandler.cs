// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for ApprovePermitWorkflowCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.PermitToWork.PermitWorkflow.Interfaces;
using PlantSafety.Domain.PermitToWork.PermitWorkflow.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PermitToWork.Commands;

/// <summary>Handles <see cref="ApprovePermitWorkflowCommand"/>.</summary>
public sealed class ApprovePermitWorkflowCommandHandler : IRequestHandler<ApprovePermitWorkflowCommand, Result<PermitWorkflowActionResult>>
{
    private readonly IPermitWorkflowRepository _repository;
    private readonly ILogger<ApprovePermitWorkflowCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ApprovePermitWorkflowCommandHandler(IPermitWorkflowRepository repository, ILogger<ApprovePermitWorkflowCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PermitWorkflowActionResult>> Handle(ApprovePermitWorkflowCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = PermitWorkflowId.FromGuid(request.PermitWorkflowId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<PermitWorkflowActionResult>.Failure(new ApplicationError(
                    "PermitToWork.PermitWorkflow.NotFound",
                    "The requested permit workflow could not be found.",
                    Guid.NewGuid()));
            }

            var approvedBy = UserId.FromGuid(request.ApprovedByUserId);

            aggregate.Approve(
                approvedBy: approvedBy,
                approvedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(approvedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            return Result<PermitWorkflowActionResult>.Success(new PermitWorkflowActionResult(
                aggregate.PermitWorkflowId.Value,
                aggregate.Status.ToString()));
        }
        catch (ConcurrencyConflictException)
        {
            // Let this propagate to ExceptionHandlingMiddleware, which maps it to HTTP 409 --
            // swallowing it here as a generic ApplicationError would misreport a stale-ETag
            // conflict as an ordinary 400 failure.
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to approve PermitWorkflow {PermitWorkflowId}", request.PermitWorkflowId);

            return Result<PermitWorkflowActionResult>.Failure(new ApplicationError(
                "PermitToWork.PermitWorkflow.ApproveFailed",
                "Unable to approve the permit at this time.",
                Guid.NewGuid()));
        }
    }
}

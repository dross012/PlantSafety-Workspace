// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for RecordPermitClosureSignatureCommand.
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

/// <summary>Handles <see cref="RecordPermitClosureSignatureCommand"/>.</summary>
public sealed class RecordPermitClosureSignatureCommandHandler : IRequestHandler<RecordPermitClosureSignatureCommand, Result<PermitWorkflowActionResult>>
{
    private readonly IPermitWorkflowRepository _repository;
    private readonly ILogger<RecordPermitClosureSignatureCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RecordPermitClosureSignatureCommandHandler(IPermitWorkflowRepository repository, ILogger<RecordPermitClosureSignatureCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PermitWorkflowActionResult>> Handle(RecordPermitClosureSignatureCommand request, CancellationToken cancellationToken)
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

            var signedBy = UserId.FromGuid(request.ClosureSignedByUserId);

            aggregate.RecordClosureSignature(
                closureSignedBy: signedBy,
                closureNotes: request.ClosureNotes,
                closedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(signedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<PermitWorkflowActionResult>.Success(new PermitWorkflowActionResult(
                aggregate.PermitWorkflowId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record closure signature for PermitWorkflow {PermitWorkflowId}", request.PermitWorkflowId);

            return Result<PermitWorkflowActionResult>.Failure(new ApplicationError(
                "PermitToWork.PermitWorkflow.RecordClosureSignatureFailed",
                "Unable to record the permit closure signature at this time.",
                Guid.NewGuid()));
        }
    }
}

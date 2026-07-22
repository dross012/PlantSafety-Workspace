// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for AssessPermitHazardsCommand. Loads the aggregate, invokes the
// domain lifecycle method, and persists the result. No business rules live here.
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

/// <summary>Handles <see cref="AssessPermitHazardsCommand"/>.</summary>
public sealed class AssessPermitHazardsCommandHandler : IRequestHandler<AssessPermitHazardsCommand, Result<PermitWorkflowActionResult>>
{
    private readonly IPermitWorkflowRepository _repository;
    private readonly ILogger<AssessPermitHazardsCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public AssessPermitHazardsCommandHandler(IPermitWorkflowRepository repository, ILogger<AssessPermitHazardsCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PermitWorkflowActionResult>> Handle(AssessPermitHazardsCommand request, CancellationToken cancellationToken)
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

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.AssessHazards(
                requiredControls: request.RequiredControls,
                riskScore: request.RiskScore,
                assessedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<PermitWorkflowActionResult>.Success(new PermitWorkflowActionResult(
                aggregate.PermitWorkflowId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to assess hazards for PermitWorkflow {PermitWorkflowId}", request.PermitWorkflowId);

            return Result<PermitWorkflowActionResult>.Failure(new ApplicationError(
                "PermitToWork.PermitWorkflow.AssessHazardsFailed",
                "Unable to record the hazard assessment at this time.",
                Guid.NewGuid()));
        }
    }
}

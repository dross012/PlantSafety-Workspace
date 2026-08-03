// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Application
//  File:           MakeAutomatedDecisionCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for MakeAutomatedDecisionCommand. Pure orchestration.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Workflow.Interfaces;
using PlantSafety.Domain.Workflow.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Workflow.Commands;

/// <summary>Handles <see cref="MakeAutomatedDecisionCommand"/>.</summary>
public sealed class MakeAutomatedDecisionCommandHandler : IRequestHandler<MakeAutomatedDecisionCommand, Result<WorkflowInstanceStatusResult>>
{
    private readonly IWorkflowInstanceRepository _repository;
    private readonly ILogger<MakeAutomatedDecisionCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public MakeAutomatedDecisionCommandHandler(IWorkflowInstanceRepository repository, ILogger<MakeAutomatedDecisionCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<WorkflowInstanceStatusResult>> Handle(MakeAutomatedDecisionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var instance = await _repository.GetByIdAsync(WorkflowInstanceId.FromGuid(request.WorkflowInstanceId), request.TenantId, cancellationToken);
            if (instance is null)
            {
                return Result<WorkflowInstanceStatusResult>.Failure(new ApplicationError(
                    "Workflow.Instance.NotFound",
                    "The requested workflow instance could not be found.",
                    request.CorrelationId,
                    ErrorSeverity.NotFound));
            }

            var requesterId = UserId.FromGuid(request.RequestedByUserId);

            instance.RecordAutomatedDecision(
                request.Approved, request.Notes, UtcTimestamp.Now(), PerformedByActor.System(),
                new CorrelationId(request.CorrelationId), new CausationId(request.CausationId));

            await _repository.UpdateAsync(instance, eTag: null, cancellationToken);

            return Result<WorkflowInstanceStatusResult>.Success(new WorkflowInstanceStatusResult(
                instance.WorkflowInstanceId.Value, instance.State.Code, instance.CurrentStepId?.Value));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record automated decision for WorkflowInstance {WorkflowInstanceId}", request.WorkflowInstanceId);

            return Result<WorkflowInstanceStatusResult>.Failure(new ApplicationError(
                "Workflow.Instance.AutomatedDecisionFailed",
                "Unable to record the automated decision at this time.",
                Guid.NewGuid()));
        }
    }
}

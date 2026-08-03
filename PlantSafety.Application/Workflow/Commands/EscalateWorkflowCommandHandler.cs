// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Application
//  File:           EscalateWorkflowCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for EscalateWorkflowCommand. Loads the WorkflowInstance and its
//                  originating WorkflowDefinition, resolves the current step's escalation target
//                  via WorkflowEscalationPolicy, and delegates the mutation to the instance
//                  aggregate. No business rules live here.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Workflow.Exceptions;
using PlantSafety.Domain.Workflow.Interfaces;
using PlantSafety.Domain.Workflow.Policies;
using PlantSafety.Domain.Workflow.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Workflow.Commands;

/// <summary>Handles <see cref="EscalateWorkflowCommand"/>.</summary>
public sealed class EscalateWorkflowCommandHandler : IRequestHandler<EscalateWorkflowCommand, Result<WorkflowInstanceStatusResult>>
{
    private readonly IWorkflowInstanceRepository _instanceRepository;
    private readonly IWorkflowDefinitionRepository _definitionRepository;
    private readonly ILogger<EscalateWorkflowCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public EscalateWorkflowCommandHandler(
        IWorkflowInstanceRepository instanceRepository,
        IWorkflowDefinitionRepository definitionRepository,
        ILogger<EscalateWorkflowCommandHandler> logger)
    {
        _instanceRepository = instanceRepository;
        _definitionRepository = definitionRepository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<WorkflowInstanceStatusResult>> Handle(EscalateWorkflowCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var instance = await _instanceRepository.GetByIdAsync(WorkflowInstanceId.FromGuid(request.WorkflowInstanceId), request.TenantId, cancellationToken);
            if (instance is null)
            {
                return Result<WorkflowInstanceStatusResult>.Failure(new ApplicationError(
                    "Workflow.Instance.NotFound",
                    "The requested workflow instance could not be found.",
                    request.CorrelationId,
                    ErrorSeverity.NotFound));
            }

            var definition = await _definitionRepository.GetByIdAsync(instance.WorkflowDefinitionId, request.TenantId, cancellationToken);
            if (definition is null || instance.CurrentStepId is not { } currentStepId)
            {
                return Result<WorkflowInstanceStatusResult>.Failure(new ApplicationError(
                    "Workflow.Instance.NoCurrentStep",
                    "The workflow instance has no current step to escalate.",
                    request.CorrelationId,
                    ErrorSeverity.Validation));
            }

            var stepDefinition = definition.FindStep(currentStepId)
                ?? throw new WorkflowStepNotFoundException(currentStepId, new CorrelationId(request.CorrelationId), new CausationId(request.CausationId));

            var escalationTarget = WorkflowEscalationPolicy.ResolveEscalationTarget(stepDefinition);
            var requesterId = UserId.FromGuid(request.RequestedByUserId);

            instance.EscalateCurrentStep(
                escalationTarget, UtcTimestamp.Now(), PerformedByActor.User(requesterId),
                new CorrelationId(request.CorrelationId), new CausationId(request.CausationId));

            await _instanceRepository.UpdateAsync(instance, eTag: null, cancellationToken);

            return Result<WorkflowInstanceStatusResult>.Success(new WorkflowInstanceStatusResult(
                instance.WorkflowInstanceId.Value, instance.State.Code, instance.CurrentStepId?.Value));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to escalate WorkflowInstance {WorkflowInstanceId}", request.WorkflowInstanceId);

            return Result<WorkflowInstanceStatusResult>.Failure(new ApplicationError(
                "Workflow.Instance.EscalateFailed",
                "Unable to escalate the workflow instance at this time.",
                Guid.NewGuid()));
        }
    }
}

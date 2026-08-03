// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Application
//  File:           RetryWorkflowStepCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for RetryWorkflowStepCommand. Loads the WorkflowInstance and its
//                  originating WorkflowDefinition, resolves the current step's configured
//                  RetryPolicy, and delegates the mutation to the instance aggregate (which
//                  deterministically computes the next-attempt UtcTimestamp -- no wall-clock
//                  sleep). No business rules live here.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Workflow.Exceptions;
using PlantSafety.Domain.Workflow.Interfaces;
using PlantSafety.Domain.Workflow.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Workflow.Commands;

/// <summary>Handles <see cref="RetryWorkflowStepCommand"/>.</summary>
public sealed class RetryWorkflowStepCommandHandler : IRequestHandler<RetryWorkflowStepCommand, Result<WorkflowInstanceStatusResult>>
{
    private readonly IWorkflowInstanceRepository _instanceRepository;
    private readonly IWorkflowDefinitionRepository _definitionRepository;
    private readonly ILogger<RetryWorkflowStepCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RetryWorkflowStepCommandHandler(
        IWorkflowInstanceRepository instanceRepository,
        IWorkflowDefinitionRepository definitionRepository,
        ILogger<RetryWorkflowStepCommandHandler> logger)
    {
        _instanceRepository = instanceRepository;
        _definitionRepository = definitionRepository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<WorkflowInstanceStatusResult>> Handle(RetryWorkflowStepCommand request, CancellationToken cancellationToken)
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
                    "The workflow instance has no current step to retry.",
                    request.CorrelationId,
                    ErrorSeverity.Validation));
            }

            var stepDefinition = definition.FindStep(currentStepId)
                ?? throw new WorkflowStepNotFoundException(currentStepId, new CorrelationId(request.CorrelationId), new CausationId(request.CausationId));

            if (stepDefinition.Retry is not { } retryPolicy)
            {
                return Result<WorkflowInstanceStatusResult>.Failure(new ApplicationError(
                    "Workflow.Instance.RetryNotConfigured",
                    "This step has no retry policy configured.",
                    request.CorrelationId,
                    ErrorSeverity.Validation));
            }

            var requesterId = UserId.FromGuid(request.RequestedByUserId);

            instance.ScheduleRetry(
                retryPolicy, UtcTimestamp.Now(), PerformedByActor.User(requesterId),
                new CorrelationId(request.CorrelationId), new CausationId(request.CausationId));

            await _instanceRepository.UpdateAsync(instance, eTag: null, cancellationToken);

            return Result<WorkflowInstanceStatusResult>.Success(new WorkflowInstanceStatusResult(
                instance.WorkflowInstanceId.Value, instance.State.Code, instance.CurrentStepId?.Value));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to schedule retry for WorkflowInstance {WorkflowInstanceId}", request.WorkflowInstanceId);

            return Result<WorkflowInstanceStatusResult>.Failure(new ApplicationError(
                "Workflow.Instance.RetryFailed",
                "Unable to schedule the retry at this time.",
                Guid.NewGuid()));
        }
    }
}

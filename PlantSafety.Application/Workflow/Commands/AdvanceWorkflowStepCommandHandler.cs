// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Application
//  File:           AdvanceWorkflowStepCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for AdvanceWorkflowStepCommand. Loads both the WorkflowInstance and its
//                  originating WorkflowDefinition and delegates to WorkflowRuntimeService (the
//                  domain service that resolves the guarded transition graph spanning both
//                  aggregates). No business rules live here.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Workflow.Interfaces;
using PlantSafety.Domain.Workflow.Services;
using PlantSafety.Domain.Workflow.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Workflow.Commands;

/// <summary>Handles <see cref="AdvanceWorkflowStepCommand"/>.</summary>
public sealed class AdvanceWorkflowStepCommandHandler : IRequestHandler<AdvanceWorkflowStepCommand, Result<WorkflowInstanceStatusResult>>
{
    private readonly IWorkflowInstanceRepository _instanceRepository;
    private readonly IWorkflowDefinitionRepository _definitionRepository;
    private readonly WorkflowRuntimeService _runtimeService;
    private readonly ILogger<AdvanceWorkflowStepCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public AdvanceWorkflowStepCommandHandler(
        IWorkflowInstanceRepository instanceRepository,
        IWorkflowDefinitionRepository definitionRepository,
        WorkflowRuntimeService runtimeService,
        ILogger<AdvanceWorkflowStepCommandHandler> logger)
    {
        _instanceRepository = instanceRepository;
        _definitionRepository = definitionRepository;
        _runtimeService = runtimeService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<WorkflowInstanceStatusResult>> Handle(AdvanceWorkflowStepCommand request, CancellationToken cancellationToken)
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
            if (definition is null)
            {
                return Result<WorkflowInstanceStatusResult>.Failure(new ApplicationError(
                    "Workflow.Definition.NotFound",
                    "The originating workflow definition could not be found.",
                    request.CorrelationId,
                    ErrorSeverity.NotFound));
            }

            if (request.ContextUpdates is not null)
                foreach (var kvp in request.ContextUpdates)
                    instance.SetContextValue(kvp.Key, kvp.Value);

            var requesterId = UserId.FromGuid(request.RequestedByUserId);

            _runtimeService.AdvanceToNextStep(
                definition, instance, UtcTimestamp.Now(), PerformedByActor.User(requesterId),
                new CorrelationId(request.CorrelationId), new CausationId(request.CausationId));

            await _instanceRepository.UpdateAsync(instance, eTag: null, cancellationToken);

            return Result<WorkflowInstanceStatusResult>.Success(new WorkflowInstanceStatusResult(
                instance.WorkflowInstanceId.Value, instance.State.Code, instance.CurrentStepId?.Value));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to advance WorkflowInstance {WorkflowInstanceId}", request.WorkflowInstanceId);

            return Result<WorkflowInstanceStatusResult>.Failure(new ApplicationError(
                "Workflow.Instance.AdvanceFailed",
                "Unable to advance the workflow instance at this time.",
                Guid.NewGuid()));
        }
    }
}

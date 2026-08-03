// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Application
//  File:           StartWorkflowCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for StartWorkflowCommand. Loads the target (already-activated)
//                  WorkflowDefinition, constructs a WorkflowInstance via its domain factory, and
//                  persists it through the domain repository interface. No business rules live
//                  here.
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

/// <summary>Handles <see cref="StartWorkflowCommand"/>.</summary>
public sealed class StartWorkflowCommandHandler : IRequestHandler<StartWorkflowCommand, Result<WorkflowInstanceStatusResult>>
{
    private readonly IWorkflowDefinitionRepository _definitionRepository;
    private readonly IWorkflowInstanceRepository _instanceRepository;
    private readonly IWorkflowInstanceFactory _instanceFactory;
    private readonly ILogger<StartWorkflowCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public StartWorkflowCommandHandler(
        IWorkflowDefinitionRepository definitionRepository,
        IWorkflowInstanceRepository instanceRepository,
        IWorkflowInstanceFactory instanceFactory,
        ILogger<StartWorkflowCommandHandler> logger)
    {
        _definitionRepository = definitionRepository;
        _instanceRepository = instanceRepository;
        _instanceFactory = instanceFactory;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<WorkflowInstanceStatusResult>> Handle(StartWorkflowCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var definition = await _definitionRepository.GetByIdAsync(WorkflowDefinitionId.FromGuid(request.WorkflowDefinitionId), request.TenantId, cancellationToken);
            if (definition is null)
            {
                return Result<WorkflowInstanceStatusResult>.Failure(new ApplicationError(
                    "Workflow.Definition.NotFound",
                    "The requested workflow definition could not be found.",
                    request.CorrelationId,
                    ErrorSeverity.NotFound));
            }

            if (!definition.IsActive)
            {
                return Result<WorkflowInstanceStatusResult>.Failure(new ApplicationError(
                    "Workflow.Definition.NotActive",
                    "The workflow definition is not active and cannot be started.",
                    request.CorrelationId,
                    ErrorSeverity.Validation));
            }

            var initiatorId = UserId.FromGuid(request.RequestedByUserId);

            var instance = _instanceFactory.Start(
                definition,
                initiatorId,
                UtcTimestamp.Now(),
                new CorrelationId(request.CorrelationId),
                new CausationId(request.CausationId),
                PerformedByActor.User(initiatorId),
                request.TenantId,
                request.SiteId,
                request.InitialContextData);

            await _instanceRepository.AddAsync(instance, cancellationToken);

            return Result<WorkflowInstanceStatusResult>.Success(new WorkflowInstanceStatusResult(
                instance.WorkflowInstanceId.Value, instance.State.Code, instance.CurrentStepId?.Value));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start WorkflowInstance for WorkflowDefinition {WorkflowDefinitionId}", request.WorkflowDefinitionId);

            return Result<WorkflowInstanceStatusResult>.Failure(new ApplicationError(
                "Workflow.Instance.StartFailed",
                "Unable to start the workflow instance at this time.",
                Guid.NewGuid()));
        }
    }
}

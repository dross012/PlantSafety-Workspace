// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Application
//  File:           ApproveStepCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for ApproveStepCommand. Pure orchestration: loads the WorkflowInstance
//                  aggregate and delegates to its own ApproveCurrentStep mutator. No business rules
//                  live here.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Workflow.Interfaces;
using PlantSafety.Domain.Workflow.ValueObjects.Ids;
using PlantSafety.Application.Abstractions.CurrentUser;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Workflow.Commands;

/// <summary>Handles <see cref="ApproveStepCommand"/>.</summary>
public sealed class ApproveStepCommandHandler : IRequestHandler<ApproveStepCommand, Result<WorkflowInstanceStatusResult>>
{
    private readonly IWorkflowInstanceRepository _repository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ApproveStepCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ApproveStepCommandHandler(
        IWorkflowInstanceRepository repository,
        ICurrentUserService currentUserService,
        ILogger<ApproveStepCommandHandler> logger)
    {
        _repository = repository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<WorkflowInstanceStatusResult>> Handle(ApproveStepCommand request, CancellationToken cancellationToken)
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

            var approverId = UserId.FromGuid(request.ApprovedByUserId);

            // Zero-Trust: the caller must be the specific user, or hold the role, assigned to the
            // step being decided. Holding the broader "workflow approve" policy is not sufficient --
            // that only grants the ability to approve steps assigned to the caller.
            var currentStep = instance.GetCurrentStepInstance(new CorrelationId(request.CorrelationId), new CausationId(request.CausationId));
            var candidateRoles = new HashSet<string>(_currentUserService.Roles, StringComparer.OrdinalIgnoreCase);
            if (!currentStep.AssignedActor.Matches(approverId, candidateRoles))
            {
                _logger.LogWarning(
                    "WorkflowApproveDenied | WorkflowInstanceId={WorkflowInstanceId} | ActorId={ActorId} | AssignedActor={AssignedActor} | CorrelationId={CorrelationId}",
                    request.WorkflowInstanceId, request.ApprovedByUserId, currentStep.AssignedActor, request.CorrelationId);

                return Result<WorkflowInstanceStatusResult>.Failure(new ApplicationError(
                    "Workflow.Instance.ApproveNotAssignedActor",
                    "You are not the actor assigned to decide this workflow step.",
                    request.CorrelationId,
                    ErrorSeverity.Authorization));
            }

            instance.ApproveCurrentStep(
                approverId, request.Notes, UtcTimestamp.Now(), PerformedByActor.User(approverId),
                new CorrelationId(request.CorrelationId), new CausationId(request.CausationId));

            await _repository.UpdateAsync(instance, eTag: null, cancellationToken);

            return Result<WorkflowInstanceStatusResult>.Success(new WorkflowInstanceStatusResult(
                instance.WorkflowInstanceId.Value, instance.State.Code, instance.CurrentStepId?.Value));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to approve current step of WorkflowInstance {WorkflowInstanceId}", request.WorkflowInstanceId);

            return Result<WorkflowInstanceStatusResult>.Failure(new ApplicationError(
                "Workflow.Instance.ApproveFailed",
                "Unable to record the approval decision at this time.",
                Guid.NewGuid()));
        }
    }
}

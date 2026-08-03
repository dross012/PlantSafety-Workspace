// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Application
//  File:           AddWorkflowStepDefinitionCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for AddWorkflowStepDefinitionCommand. Loads the target
//                  WorkflowDefinition, adds the step via the aggregate's own mutator, and
//                  persists the change. No business rules live here.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Workflow.Enums;
using PlantSafety.Domain.Workflow.Interfaces;
using PlantSafety.Domain.Workflow.ValueObjects;
using PlantSafety.Domain.Workflow.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Workflow.Commands;

/// <summary>Handles <see cref="AddWorkflowStepDefinitionCommand"/>.</summary>
public sealed class AddWorkflowStepDefinitionCommandHandler : IRequestHandler<AddWorkflowStepDefinitionCommand, Result<AddWorkflowStepDefinitionResult>>
{
    private readonly IWorkflowDefinitionRepository _repository;
    private readonly ILogger<AddWorkflowStepDefinitionCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public AddWorkflowStepDefinitionCommandHandler(IWorkflowDefinitionRepository repository, ILogger<AddWorkflowStepDefinitionCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AddWorkflowStepDefinitionResult>> Handle(AddWorkflowStepDefinitionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var definition = await _repository.GetByIdAsync(WorkflowDefinitionId.FromGuid(request.WorkflowDefinitionId), request.TenantId, cancellationToken);
            if (definition is null)
            {
                return Result<AddWorkflowStepDefinitionResult>.Failure(new ApplicationError(
                    "Workflow.Definition.NotFound",
                    "The requested workflow definition could not be found.",
                    request.CorrelationId,
                    ErrorSeverity.NotFound));
            }

            if (!WorkflowStepType.TryParse(request.StepTypeCode, out var stepType))
            {
                return Result<AddWorkflowStepDefinitionResult>.Failure(new ApplicationError(
                    "Workflow.Definition.InvalidStepType",
                    $"'{request.StepTypeCode}' is not a recognized workflow step type.",
                    request.CorrelationId,
                    ErrorSeverity.Validation));
            }

            var assignedActor = request.AssignedUserId is { } uid
                ? AssignedActor.ForUser(UserId.FromGuid(uid))
                : request.AssignedRoleCode is { Length: > 0 } role
                    ? AssignedActor.ForRole(role)
                    : AssignedActor.Unassigned;

            var escalationActor = request.EscalationRoleCode is { Length: > 0 } escRole
                ? AssignedActor.ForRole(escRole)
                : AssignedActor.Unassigned;

            var entryCondition = string.IsNullOrWhiteSpace(request.EntryConditionField)
                ? GuardCondition.None
                : GuardCondition.Create(
                    request.EntryConditionField,
                    Enum.Parse<GuardOperator>(request.EntryConditionOperatorCode, ignoreCase: true),
                    request.EntryConditionExpectedValue ?? string.Empty);

            TimeoutPolicy? timeout = request.TimeoutSeconds is { } secs
                ? new TimeoutPolicy(TimeSpan.FromSeconds(secs), Enum.Parse<TimeoutAction>(request.TimeoutActionCode, ignoreCase: true))
                : null;

            RetryPolicy? retry = request.RetryMaxAttempts is { } maxAttempts
                ? new RetryPolicy(maxAttempts, RetryBackoffStrategy.From(request.RetryBackoffStrategyCode), TimeSpan.FromSeconds(request.RetryBaseDelaySeconds))
                : null;

            var requesterId = UserId.FromGuid(request.RequestedByUserId);

            var step = definition.AddStep(
                request.Order,
                request.Name,
                stepType,
                assignedActor,
                escalationActor,
                entryCondition,
                timeout,
                retry,
                UtcTimestamp.Now(),
                PerformedByActor.User(requesterId),
                new CorrelationId(request.CorrelationId),
                new CausationId(request.CausationId));

            await _repository.UpdateAsync(definition, eTag: null, cancellationToken);

            return Result<AddWorkflowStepDefinitionResult>.Success(new AddWorkflowStepDefinitionResult(
                definition.WorkflowDefinitionId.Value, step.StepId.Value));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add step to WorkflowDefinition {WorkflowDefinitionId}", request.WorkflowDefinitionId);

            return Result<AddWorkflowStepDefinitionResult>.Failure(new ApplicationError(
                "Workflow.Definition.AddStepFailed",
                "Unable to add the step to the workflow definition at this time.",
                Guid.NewGuid()));
        }
    }
}

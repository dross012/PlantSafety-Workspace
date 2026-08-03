// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Workflow
//  Layer:          Application
//  File:           CreateWorkflowDefinitionCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CreateWorkflowDefinitionCommand. Pure orchestration: constructs the
//                  WorkflowDefinition aggregate via its domain factory method and persists it
//                  through the domain repository interface. No business rules live here.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Workflow.Enums;
using PlantSafety.Domain.Workflow.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Workflow.Commands;

/// <summary>Handles <see cref="CreateWorkflowDefinitionCommand"/>.</summary>
public sealed class CreateWorkflowDefinitionCommandHandler : IRequestHandler<CreateWorkflowDefinitionCommand, Result<CreateWorkflowDefinitionResult>>
{
    private readonly IWorkflowDefinitionRepository _repository;
    private readonly ILogger<CreateWorkflowDefinitionCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateWorkflowDefinitionCommandHandler(IWorkflowDefinitionRepository repository, ILogger<CreateWorkflowDefinitionCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CreateWorkflowDefinitionResult>> Handle(CreateWorkflowDefinitionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (!WorkflowType.TryParse(request.WorkflowTypeCode, out var workflowType))
            {
                return Result<CreateWorkflowDefinitionResult>.Failure(new ApplicationError(
                    "Workflow.Definition.InvalidWorkflowType",
                    $"'{request.WorkflowTypeCode}' is not a recognized workflow type.",
                    request.CorrelationId,
                    ErrorSeverity.Validation));
            }

            var requesterId = UserId.FromGuid(request.RequestedByUserId);

            var aggregate = Domain.Workflow.Aggregates.WorkflowDefinition.Create(
                createdBy: requesterId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(requesterId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                workflowType: workflowType,
                name: request.Name,
                description: request.Description);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CreateWorkflowDefinitionResult>.Success(new CreateWorkflowDefinitionResult(
                aggregate.WorkflowDefinitionId.Value, aggregate.IsActive));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create WorkflowDefinition for Tenant {TenantId}", request.TenantId);

            return Result<CreateWorkflowDefinitionResult>.Failure(new ApplicationError(
                "Workflow.Definition.CreateFailed",
                "Unable to create the workflow definition at this time.",
                Guid.NewGuid()));
        }
    }
}

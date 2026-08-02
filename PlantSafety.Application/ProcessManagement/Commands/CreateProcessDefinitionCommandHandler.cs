// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement
//  Layer:          Application
//  File:           CreateProcessDefinitionCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CreateProcessDefinitionCommand. Pure orchestration: constructs the
//                  ProcessDefinition aggregate via its domain factory method and persists it
//                  through the domain repository interface. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Commands.CreateAuditingCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.ProcessManagement.Repositories;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.ProcessManagement.Commands;

/// <summary>Handles <see cref="CreateProcessDefinitionCommand"/>.</summary>
public sealed class CreateProcessDefinitionCommandHandler : IRequestHandler<CreateProcessDefinitionCommand, Result<CreateProcessDefinitionResult>>
{
    private readonly IProcessDefinitionRepository _repository;
    private readonly ILogger<CreateProcessDefinitionCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateProcessDefinitionCommandHandler(IProcessDefinitionRepository repository, ILogger<CreateProcessDefinitionCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CreateProcessDefinitionResult>> Handle(CreateProcessDefinitionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var requesterId = UserId.FromGuid(request.RequesterUserId);
            var ownerId = UserId.FromGuid(request.OwnerId);

            var aggregate = PlantSafety.Domain.ProcessManagement.Aggregates.ProcessDefinition.Create(
                tenantId: request.TenantId,
                siteId: request.SiteId,
                processName: request.ProcessName,
                category: request.Category,
                ownerId: ownerId,
                createdBy: requesterId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(requesterId));

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CreateProcessDefinitionResult>.Success(new CreateProcessDefinitionResult(
                aggregate.ProcessDefinitionId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create ProcessDefinition for Tenant {TenantId}", request.TenantId);

            return Result<CreateProcessDefinitionResult>.Failure(new ApplicationError(
                "ProcessManagement.ProcessDefinition.CreateFailed",
                "Unable to submit the process definition at this time.",
                Guid.NewGuid()));
        }
    }
}

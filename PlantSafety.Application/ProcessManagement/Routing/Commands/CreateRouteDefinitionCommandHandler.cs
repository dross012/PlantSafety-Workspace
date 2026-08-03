// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Application
//  File:           CreateRouteDefinitionCommandHandler.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Handler for CreateRouteDefinitionCommand. Constructs the RouteDefinition
//                  aggregate via its domain factory method, captures the initial (Pending)
//                  RouteVersionSnapshot for v1, and persists both through their respective domain
//                  repository interfaces. Mirrors
//                  PlantSafety.Application.ProcessManagement.Commands.CreateProcessDefinitionCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.ProcessManagement.Routing.Aggregates;
using PlantSafety.Domain.ProcessManagement.Routing.Enums;
using PlantSafety.Domain.ProcessManagement.Routing.Repositories;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.ProcessManagement.Routing.Commands;

/// <summary>Handles <see cref="CreateRouteDefinitionCommand"/>.</summary>
public sealed class CreateRouteDefinitionCommandHandler : IRequestHandler<CreateRouteDefinitionCommand, Result<CreateRouteDefinitionResult>>
{
    private readonly IRouteDefinitionRepository _repository;
    private readonly IRouteVersionSnapshotRepository _snapshotRepository;
    private readonly ILogger<CreateRouteDefinitionCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateRouteDefinitionCommandHandler(
        IRouteDefinitionRepository repository,
        IRouteVersionSnapshotRepository snapshotRepository,
        ILogger<CreateRouteDefinitionCommandHandler> logger)
    {
        _repository = repository;
        _snapshotRepository = snapshotRepository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CreateRouteDefinitionResult>> Handle(CreateRouteDefinitionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var requesterId = UserId.FromGuid(request.RequesterUserId);
            var ownerId = UserId.FromGuid(request.OwnerId);
            var now = UtcTimestamp.Now();

            var steps = request.Steps
                .Select(s => (s.Name, s.Instructions, s.Resources, s.EstimatedDurationMinutes))
                .ToList();

            var aggregate = RouteDefinition.Create(
                tenantId: request.TenantId,
                siteId: request.SiteId,
                plantName: request.PlantName,
                name: request.Name,
                steps: steps,
                ownerId: ownerId,
                ownerName: request.OwnerName,
                effectiveFromUtc: UtcTimestamp.From(request.EffectiveFromUtc),
                effectiveToUtc: request.EffectiveToUtc.HasValue ? UtcTimestamp.From(request.EffectiveToUtc.Value) : null,
                createdBy: requesterId,
                createdAtUtc: now,
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(requesterId));

            await _repository.AddAsync(aggregate, cancellationToken);

            var snapshot = RouteVersionSnapshot.Capture(
                aggregate.RouteDefinitionId, aggregate.TenantId, aggregate.Version,
                aggregate.ToStepSnapshots(), RouteVersionStatus.Pending,
                aggregate.EffectiveFromUtc, now);

            await _snapshotRepository.AddAsync(snapshot, cancellationToken);

            return Result<CreateRouteDefinitionResult>.Success(new CreateRouteDefinitionResult(
                aggregate.RouteDefinitionId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create RouteDefinition for Tenant {TenantId}", request.TenantId);

            return Result<CreateRouteDefinitionResult>.Failure(new ApplicationError(
                "ProcessManagement.Routing.RouteDefinition.CreateFailed",
                "Unable to submit the route definition at this time.",
                Guid.NewGuid()));
        }
    }
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Application
//  File:           ApproveRouteDefinitionCommandHandler.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Handler for ApproveRouteDefinitionCommand. Approves the RouteDefinition
//                  aggregate and, additively, marks the current (latest) RouteVersionSnapshot as
//                  Approved -- the one piece of behaviour genuinely new relative to
//                  ApproveProcessDefinitionCommandHandler, since ProcessDefinition has no
//                  persisted version-snapshot to update in step. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.ProcessManagement.Routing.Repositories;
using PlantSafety.Domain.ProcessManagement.Routing.ValueObjects;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.ProcessManagement.Routing.Commands;

/// <summary>Handles <see cref="ApproveRouteDefinitionCommand"/>.</summary>
public sealed class ApproveRouteDefinitionCommandHandler : IRequestHandler<ApproveRouteDefinitionCommand, Result<RouteDefinitionStatusResult>>
{
    private readonly IRouteDefinitionRepository _repository;
    private readonly IRouteVersionSnapshotRepository _snapshotRepository;
    private readonly ILogger<ApproveRouteDefinitionCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ApproveRouteDefinitionCommandHandler(
        IRouteDefinitionRepository repository,
        IRouteVersionSnapshotRepository snapshotRepository,
        ILogger<ApproveRouteDefinitionCommandHandler> logger)
    {
        _repository = repository;
        _snapshotRepository = snapshotRepository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RouteDefinitionStatusResult>> Handle(ApproveRouteDefinitionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = RouteDefinitionId.FromGuid(request.RouteDefinitionId);
            var aggregate = await _repository.GetByIdAsync(id, request.TenantId, cancellationToken);
            if (aggregate is null)
            {
                return Result<RouteDefinitionStatusResult>.Failure(new ApplicationError(
                    "ProcessManagement.Routing.RouteDefinition.NotFound",
                    "The requested route definition could not be found.",
                    request.RouteDefinitionId,
                    ErrorSeverity.NotFound));
            }

            var approvedBy = UserId.FromGuid(request.ApprovedByUserId);

            aggregate.Approve(
                approvedBy: approvedBy,
                approvedAtUtc: UtcTimestamp.Now(),
                actor: PerformedByActor.User(approvedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, request.ETag, cancellationToken);

            var latestSnapshot = await _snapshotRepository.GetLatestForRouteAsync(id, request.TenantId, cancellationToken);
            if (latestSnapshot is not null)
            {
                latestSnapshot.MarkApproved();
                await _snapshotRepository.UpdateStatusAsync(latestSnapshot, cancellationToken);
            }

            return Result<RouteDefinitionStatusResult>.Success(new RouteDefinitionStatusResult(
                aggregate.RouteDefinitionId.Value, aggregate.Status.ToString(), aggregate.Version));
        }
        catch (ConcurrencyConflictException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to approve RouteDefinition {RouteDefinitionId}", request.RouteDefinitionId);

            return Result<RouteDefinitionStatusResult>.Failure(new ApplicationError(
                "ProcessManagement.Routing.RouteDefinition.ApproveFailed",
                "Unable to approve the route definition at this time.",
                Guid.NewGuid()));
        }
    }
}

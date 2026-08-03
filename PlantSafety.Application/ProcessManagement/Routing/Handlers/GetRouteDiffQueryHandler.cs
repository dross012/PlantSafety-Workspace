// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         ProcessManagement.Routing
//  Layer:          Application
//  File:           GetRouteDiffQueryHandler.cs
//  Created:        2026-08-03
//  SchemaVersion:  2026.1
//  Description:    Handler for GetRouteDiffQuery. Loads both versions' captured step snapshots
//                  and hands them to the pure Domain-layer RouteDiffEngine for deterministic
//                  structural comparison -- this handler performs no diff logic itself.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Domain.ProcessManagement.Routing.Services;
using PlantSafety.Application.Common.Constants;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.ProcessManagement.Routing.Models;
using PlantSafety.Application.ProcessManagement.Routing.Queries;
using PlantSafety.Application.ProcessManagement.Routing.Repositories;

namespace PlantSafety.Application.ProcessManagement.Routing.Handlers;

/// <summary>Handles <see cref="GetRouteDiffQuery"/>.</summary>
public sealed class GetRouteDiffQueryHandler : IRequestHandler<GetRouteDiffQuery, Result<PaginatedResult<RouteDiffRowDto>>>
{
    private readonly IRouteVersionSnapshotReadRepository _repository;
    private readonly ILogger<GetRouteDiffQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetRouteDiffQueryHandler(IRouteVersionSnapshotReadRepository repository, ILogger<GetRouteDiffQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PaginatedResult<RouteDiffRowDto>>> Handle(GetRouteDiffQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var previousSteps = await _repository.GetStepSnapshotsAsync(request.TenantId, request.RouteDefinitionId, request.PreviousVersionId, cancellationToken);
            if (previousSteps is null)
            {
                return Result<PaginatedResult<RouteDiffRowDto>>.Failure(new ApplicationError(
                    "ProcessManagement.Routing.RouteVersionSnapshot.NotFound",
                    "The requested previous version could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            var currentSteps = await _repository.GetStepSnapshotsAsync(request.TenantId, request.RouteDefinitionId, request.CurrentVersionId, cancellationToken);
            if (currentSteps is null)
            {
                return Result<PaginatedResult<RouteDiffRowDto>>.Failure(new ApplicationError(
                    "ProcessManagement.Routing.RouteVersionSnapshot.NotFound",
                    "The requested current version could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            var diff = RouteDiffEngine.Diff(previousSteps, currentSteps);

            var rows = diff.Select(d => new RouteDiffRowDto
            {
                DiffGroup = d.DiffGroup,
                FieldName = d.FieldName,
                ChangeType = d.ChangeType,
                PreviousValue = d.PreviousValue,
                CurrentValue = d.CurrentValue,
            }).ToList();

            var pageNumber = PaginationConstants.DefaultPageNumber;
            var pageSize = Math.Max(rows.Count, PaginationConstants.DefaultPageSize);

            return Result<PaginatedResult<RouteDiffRowDto>>.Success(new PaginatedResult<RouteDiffRowDto>(rows, rows.Count, pageNumber, pageSize));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compute diff for RouteDefinition {RouteDefinitionId} between {Previous} and {Current}",
                request.RouteDefinitionId, request.PreviousVersionId, request.CurrentVersionId);

            return Result<PaginatedResult<RouteDiffRowDto>>.Failure(new ApplicationError(
                "ProcessManagement.Routing.RouteVersionSnapshot.DiffFailed",
                "Unable to compute the route version diff at this time.",
                Guid.NewGuid()));
        }
    }
}

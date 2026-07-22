// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetControlMeasuresHierarchyQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.RiskManagement.ControlMeasuresHierarchy.Queries;
using PlantSafety.Application.RiskManagement.ControlMeasuresHierarchy.Repositories;

namespace PlantSafety.Application.RiskManagement.ControlMeasuresHierarchy.Handlers;

/// <summary>Handles <see cref="GetControlMeasuresHierarchyQuery"/>.</summary>
public sealed class GetControlMeasuresHierarchyQueryHandler : IRequestHandler<GetControlMeasuresHierarchyQuery, Result<ControlMeasuresHierarchyQueryResult>>
{
    private readonly IControlMeasuresHierarchyRepository _repository;
    private readonly ILogger<GetControlMeasuresHierarchyQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetControlMeasuresHierarchyQueryHandler(IControlMeasuresHierarchyRepository repository, ILogger<GetControlMeasuresHierarchyQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ControlMeasuresHierarchyQueryResult>> Handle(GetControlMeasuresHierarchyQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetControlMeasuresHierarchyAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<ControlMeasuresHierarchyQueryResult>.Success(new ControlMeasuresHierarchyQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve ControlMeasuresHierarchy for Tenant {TenantId}", request.TenantId);

            return Result<ControlMeasuresHierarchyQueryResult>.Failure(new ApplicationError(
                "RiskManagement.ControlMeasuresHierarchy.RetrievalFailed",
                "Unable to retrieve ControlMeasuresHierarchy data at this time.",
                Guid.NewGuid()));
        }
    }
}
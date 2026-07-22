// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetWorkingAtHeightsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Checklists.ChecklistTemplate.WorkingAtHeights.Queries;
using PlantSafety.Application.Checklists.ChecklistTemplate.WorkingAtHeights.Repositories;

namespace PlantSafety.Application.Checklists.ChecklistTemplate.WorkingAtHeights.Handlers;

/// <summary>Handles <see cref="GetWorkingAtHeightsQuery"/>.</summary>
public sealed class GetWorkingAtHeightsQueryHandler : IRequestHandler<GetWorkingAtHeightsQuery, Result<WorkingAtHeightsQueryResult>>
{
    private readonly IWorkingAtHeightsRepository _repository;
    private readonly ILogger<GetWorkingAtHeightsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetWorkingAtHeightsQueryHandler(IWorkingAtHeightsRepository repository, ILogger<GetWorkingAtHeightsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<WorkingAtHeightsQueryResult>> Handle(GetWorkingAtHeightsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<WorkingAtHeightsQueryResult>.Success(new WorkingAtHeightsQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve WorkingAtHeights for Tenant {TenantId}", request.TenantId);

            return Result<WorkingAtHeightsQueryResult>.Failure(new ApplicationError(
                "Checklists.ChecklistTemplate.WorkingAtHeights.RetrievalFailed",
                "Unable to retrieve Working at Heights Checklists data at this time.",
                Guid.NewGuid()));
        }
    }
}
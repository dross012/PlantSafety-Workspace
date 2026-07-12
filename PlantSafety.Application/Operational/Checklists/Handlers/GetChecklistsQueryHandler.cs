// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetChecklistsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.Checklists.Queries;
using PlantSafety.Application.Operational.Checklists.Repositories;

namespace PlantSafety.Application.Operational.Checklists.Handlers;

/// <summary>Handles <see cref="GetChecklistsQuery"/>.</summary>
public sealed class GetChecklistsQueryHandler : IRequestHandler<GetChecklistsQuery, Result<ChecklistsQueryResult>>
{
    private readonly IChecklistsRepository _repository;
    private readonly ILogger<GetChecklistsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetChecklistsQueryHandler(IChecklistsRepository repository, ILogger<GetChecklistsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ChecklistsQueryResult>> Handle(GetChecklistsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetChecklistsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<ChecklistsQueryResult>.Success(new ChecklistsQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Checklists for Tenant {TenantId}", request.TenantId);

            return Result<ChecklistsQueryResult>.Failure(new ApplicationError(
                "Operational.Checklists.RetrievalFailed",
                "Unable to retrieve Checklists data at this time.",
                Guid.NewGuid()));
        }
    }
}

// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetWorkatHeightQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Permits.WorkatHeight.Queries;
using PlantSafety.Application.Compliance.Permits.WorkatHeight.Repositories;

namespace PlantSafety.Application.Compliance.Permits.WorkatHeight.Handlers;

/// <summary>Handles <see cref="GetWorkatHeightQuery"/>.</summary>
public sealed class GetWorkatHeightQueryHandler : IRequestHandler<GetWorkatHeightQuery, Result<WorkatHeightQueryResult>>
{
    private readonly IWorkatHeightRepository _repository;
    private readonly ILogger<GetWorkatHeightQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetWorkatHeightQueryHandler(IWorkatHeightRepository repository, ILogger<GetWorkatHeightQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<WorkatHeightQueryResult>> Handle(GetWorkatHeightQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetWorkatHeightAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<WorkatHeightQueryResult>.Success(new WorkatHeightQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve WorkatHeight for Tenant {TenantId}", request.TenantId);

            return Result<WorkatHeightQueryResult>.Failure(new ApplicationError(
                "Compliance.Permits.WorkatHeight.RetrievalFailed",
                "Unable to retrieve work-at-height permit data at this time.",
                Guid.NewGuid()));
        }
    }
}

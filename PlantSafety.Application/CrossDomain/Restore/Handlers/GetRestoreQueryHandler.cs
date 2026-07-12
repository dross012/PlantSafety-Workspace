// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetRestoreQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.CrossDomain.Restore.Queries;
using PlantSafety.Application.CrossDomain.Restore.Repositories;

namespace PlantSafety.Application.CrossDomain.Restore.Handlers;

/// <summary>Handles <see cref="GetRestoreQuery"/>.</summary>
public sealed class GetRestoreQueryHandler : IRequestHandler<GetRestoreQuery, Result<RestoreQueryResult>>
{
    private readonly IRestoreRepository _repository;
    private readonly ILogger<GetRestoreQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetRestoreQueryHandler(IRestoreRepository repository, ILogger<GetRestoreQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RestoreQueryResult>> Handle(GetRestoreQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetRestoreJobsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<RestoreQueryResult>.Success(new RestoreQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Restore for Tenant {TenantId}", request.TenantId);

            return Result<RestoreQueryResult>.Failure(new ApplicationError(
                "CrossDomain.Restore.RetrievalFailed",
                "Unable to retrieve restore data at this time.",
                Guid.NewGuid()));
        }
    }
}

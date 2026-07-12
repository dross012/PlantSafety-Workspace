// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetCleanupQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.CrossDomain.Cleanup.Queries;
using PlantSafety.Application.CrossDomain.Cleanup.Repositories;

namespace PlantSafety.Application.CrossDomain.Cleanup.Handlers;

/// <summary>Handles <see cref="GetCleanupQuery"/>.</summary>
public sealed class GetCleanupQueryHandler : IRequestHandler<GetCleanupQuery, Result<CleanupQueryResult>>
{
    private readonly ICleanupRepository _repository;
    private readonly ILogger<GetCleanupQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetCleanupQueryHandler(ICleanupRepository repository, ILogger<GetCleanupQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CleanupQueryResult>> Handle(GetCleanupQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetCleanupTasksAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<CleanupQueryResult>.Success(new CleanupQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Cleanup for Tenant {TenantId}", request.TenantId);

            return Result<CleanupQueryResult>.Failure(new ApplicationError(
                "CrossDomain.Cleanup.RetrievalFailed",
                "Unable to retrieve cleanup data at this time.",
                Guid.NewGuid()));
        }
    }
}

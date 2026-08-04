// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for ListOpenManifestsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.HazardousWasteManagement.Manifests.Queries;
using PlantSafety.Application.HazardousWasteManagement.Manifests.Repositories;

namespace PlantSafety.Application.HazardousWasteManagement.Manifests.Handlers;

/// <summary>Handles <see cref="ListOpenManifestsQuery"/>.</summary>
public sealed class ListOpenManifestsQueryHandler : IRequestHandler<ListOpenManifestsQuery, Result<ListOpenManifestsResult>>
{
    private readonly IHazardousWasteManifestRepository _repository;
    private readonly ILogger<ListOpenManifestsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ListOpenManifestsQueryHandler(IHazardousWasteManifestRepository repository, ILogger<ListOpenManifestsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ListOpenManifestsResult>> Handle(ListOpenManifestsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.ListOpenManifestsAsync(request.TenantId, request.SiteId, cancellationToken);
            return Result<ListOpenManifestsResult>.Success(new ListOpenManifestsResult(items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list open manifests for Tenant {TenantId}", request.TenantId);

            return Result<ListOpenManifestsResult>.Failure(new ApplicationError(
                "HazardousWasteManagement.Manifest.ListOpenFailed",
                "Unable to list open manifests at this time.",
                Guid.NewGuid()));
        }
    }
}

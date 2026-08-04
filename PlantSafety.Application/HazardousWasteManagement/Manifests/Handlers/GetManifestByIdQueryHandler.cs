// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for GetManifestByIdQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.HazardousWasteManagement.Manifests.Queries;
using PlantSafety.Application.HazardousWasteManagement.Manifests.Repositories;

namespace PlantSafety.Application.HazardousWasteManagement.Manifests.Handlers;

/// <summary>Handles <see cref="GetManifestByIdQuery"/>.</summary>
public sealed class GetManifestByIdQueryHandler : IRequestHandler<GetManifestByIdQuery, Result<GetManifestByIdResult>>
{
    private readonly IHazardousWasteManifestRepository _repository;
    private readonly ILogger<GetManifestByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetManifestByIdQueryHandler(IHazardousWasteManifestRepository repository, ILogger<GetManifestByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<GetManifestByIdResult>> Handle(GetManifestByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetManifestByIdAsync(request.TenantId, request.HazardousWasteManifestId, cancellationToken);
            return Result<GetManifestByIdResult>.Success(new GetManifestByIdResult(item));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Manifest {HazardousWasteManifestId}", request.HazardousWasteManifestId);

            return Result<GetManifestByIdResult>.Failure(new ApplicationError(
                "HazardousWasteManagement.Manifest.RetrievalFailed",
                "Unable to retrieve the manifest at this time.",
                Guid.NewGuid()));
        }
    }
}

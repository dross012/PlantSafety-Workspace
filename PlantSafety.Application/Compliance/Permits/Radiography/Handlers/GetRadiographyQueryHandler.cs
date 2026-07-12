// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetRadiographyQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Permits.Radiography.Queries;
using PlantSafety.Application.Compliance.Permits.Radiography.Repositories;

namespace PlantSafety.Application.Compliance.Permits.Radiography.Handlers;

/// <summary>Handles <see cref="GetRadiographyQuery"/>.</summary>
public sealed class GetRadiographyQueryHandler : IRequestHandler<GetRadiographyQuery, Result<RadiographyQueryResult>>
{
    private readonly IRadiographyRepository _repository;
    private readonly ILogger<GetRadiographyQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetRadiographyQueryHandler(IRadiographyRepository repository, ILogger<GetRadiographyQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RadiographyQueryResult>> Handle(GetRadiographyQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetRadiographyAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<RadiographyQueryResult>.Success(new RadiographyQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Radiography for Tenant {TenantId}", request.TenantId);

            return Result<RadiographyQueryResult>.Failure(new ApplicationError(
                "Compliance.Permits.Radiography.RetrievalFailed",
                "Unable to retrieve radiography permit data at this time.",
                Guid.NewGuid()));
        }
    }
}

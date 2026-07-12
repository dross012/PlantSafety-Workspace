// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetCultureIndexQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Analytics.Culture.CultureIndex.Queries;
using PlantSafety.Application.Analytics.Culture.CultureIndex.Repositories;

namespace PlantSafety.Application.Analytics.Culture.CultureIndex.Handlers;

/// <summary>Handles <see cref="GetCultureIndexQuery"/>.</summary>
public sealed class GetCultureIndexQueryHandler : IRequestHandler<GetCultureIndexQuery, Result<CultureIndexQueryResult>>
{
    private readonly ICultureIndexRepository _repository;
    private readonly ILogger<GetCultureIndexQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetCultureIndexQueryHandler(ICultureIndexRepository repository, ILogger<GetCultureIndexQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CultureIndexQueryResult>> Handle(GetCultureIndexQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetCultureIndexSnapshotsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<CultureIndexQueryResult>.Success(new CultureIndexQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve CultureIndex for Tenant {TenantId}", request.TenantId);

            return Result<CultureIndexQueryResult>.Failure(new ApplicationError(
                "Analytics.Culture.CultureIndex.RetrievalFailed",
                "Unable to retrieve CultureIndex data at this time.",
                Guid.NewGuid()));
        }
    }
}

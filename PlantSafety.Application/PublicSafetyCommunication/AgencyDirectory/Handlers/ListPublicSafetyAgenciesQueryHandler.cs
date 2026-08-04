// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for ListPublicSafetyAgenciesQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.PublicSafetyCommunication.AgencyDirectory.Queries;
using PlantSafety.Application.PublicSafetyCommunication.AgencyDirectory.Repositories;

namespace PlantSafety.Application.PublicSafetyCommunication.AgencyDirectory.Handlers;

/// <summary>Handles <see cref="ListPublicSafetyAgenciesQuery"/>.</summary>
public sealed class ListPublicSafetyAgenciesQueryHandler : IRequestHandler<ListPublicSafetyAgenciesQuery, Result<ListPublicSafetyAgenciesQueryResult>>
{
    private readonly IPublicSafetyAgencyRepository _repository;
    private readonly ILogger<ListPublicSafetyAgenciesQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ListPublicSafetyAgenciesQueryHandler(IPublicSafetyAgencyRepository repository, ILogger<ListPublicSafetyAgenciesQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ListPublicSafetyAgenciesQueryResult>> Handle(ListPublicSafetyAgenciesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.ListPublicSafetyAgenciesAsync(request.TenantId, request.SiteId, request.AgencyType, request.ActiveOnly, cancellationToken);

            return Result<ListPublicSafetyAgenciesQueryResult>.Success(new ListPublicSafetyAgenciesQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list PublicSafetyAgency directory for Tenant {TenantId}", request.TenantId);

            return Result<ListPublicSafetyAgenciesQueryResult>.Failure(new ApplicationError(
                "PublicSafetyCommunication.PublicSafetyAgency.ListFailed",
                "Unable to retrieve the public-safety agency directory at this time.",
                Guid.NewGuid()));
        }
    }
}

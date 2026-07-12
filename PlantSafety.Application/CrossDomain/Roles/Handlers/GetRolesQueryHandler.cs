// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetRolesQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.CrossDomain.Roles.Queries;
using PlantSafety.Application.CrossDomain.Roles.Repositories;

namespace PlantSafety.Application.CrossDomain.Roles.Handlers;

/// <summary>Handles <see cref="GetRolesQuery"/>.</summary>
public sealed class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, Result<RolesQueryResult>>
{
    private readonly IRolesRepository _repository;
    private readonly ILogger<GetRolesQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetRolesQueryHandler(IRolesRepository repository, ILogger<GetRolesQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RolesQueryResult>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetRolesAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<RolesQueryResult>.Success(new RolesQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Roles for Tenant {TenantId}", request.TenantId);

            return Result<RolesQueryResult>.Failure(new ApplicationError(
                "CrossDomain.Roles.RetrievalFailed",
                "Unable to retrieve roles data at this time.",
                Guid.NewGuid()));
        }
    }
}

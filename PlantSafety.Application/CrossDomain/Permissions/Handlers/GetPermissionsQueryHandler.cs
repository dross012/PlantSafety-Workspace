// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetPermissionsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.CrossDomain.Permissions.Queries;
using PlantSafety.Application.CrossDomain.Permissions.Repositories;

namespace PlantSafety.Application.CrossDomain.Permissions.Handlers;

/// <summary>Handles <see cref="GetPermissionsQuery"/>.</summary>
public sealed class GetPermissionsQueryHandler : IRequestHandler<GetPermissionsQuery, Result<PermissionsQueryResult>>
{
    private readonly IPermissionsRepository _repository;
    private readonly ILogger<GetPermissionsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPermissionsQueryHandler(IPermissionsRepository repository, ILogger<GetPermissionsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PermissionsQueryResult>> Handle(GetPermissionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetPermissionsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<PermissionsQueryResult>.Success(new PermissionsQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Permissions for Tenant {TenantId}", request.TenantId);

            return Result<PermissionsQueryResult>.Failure(new ApplicationError(
                "CrossDomain.Permissions.RetrievalFailed",
                "Unable to retrieve permissions data at this time.",
                Guid.NewGuid()));
        }
    }
}

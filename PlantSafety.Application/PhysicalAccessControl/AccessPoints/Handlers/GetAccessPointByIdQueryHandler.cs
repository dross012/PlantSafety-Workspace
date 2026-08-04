// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for GetAccessPointByIdQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.PhysicalAccessControl.AccessPoints.Queries;
using PlantSafety.Application.PhysicalAccessControl.AccessPoints.Repositories;

namespace PlantSafety.Application.PhysicalAccessControl.AccessPoints.Handlers;

/// <summary>Handles <see cref="GetAccessPointByIdQuery"/>.</summary>
public sealed class GetAccessPointByIdQueryHandler : IRequestHandler<GetAccessPointByIdQuery, Result<GetAccessPointByIdResult>>
{
    private readonly IAccessPointReadRepository _repository;
    private readonly ILogger<GetAccessPointByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetAccessPointByIdQueryHandler(IAccessPointReadRepository repository, ILogger<GetAccessPointByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<GetAccessPointByIdResult>> Handle(GetAccessPointByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var accessPoint = await _repository.GetByIdAsync(request.TenantId, request.AccessPointId, cancellationToken);

            return Result<GetAccessPointByIdResult>.Success(new GetAccessPointByIdResult(accessPoint));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve AccessPoint {AccessPointId} for Tenant {TenantId}", request.AccessPointId, request.TenantId);

            return Result<GetAccessPointByIdResult>.Failure(new ApplicationError(
                "PhysicalAccessControl.AccessPoint.RetrievalFailed",
                "Unable to retrieve the AccessPoint record at this time.",
                Guid.NewGuid()));
        }
    }
}

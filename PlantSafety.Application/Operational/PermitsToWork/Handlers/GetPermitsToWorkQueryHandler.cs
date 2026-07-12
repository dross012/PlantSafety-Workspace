// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetPermitsToWorkQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.PermitsToWork.Queries;
using PlantSafety.Application.Operational.PermitsToWork.Repositories;

namespace PlantSafety.Application.Operational.PermitsToWork.Handlers;

/// <summary>Handles <see cref="GetPermitsToWorkQuery"/>.</summary>
public sealed class GetPermitsToWorkQueryHandler : IRequestHandler<GetPermitsToWorkQuery, Result<PermitsToWorkQueryResult>>
{
    private readonly IPermitsToWorkRepository _repository;
    private readonly ILogger<GetPermitsToWorkQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPermitsToWorkQueryHandler(IPermitsToWorkRepository repository, ILogger<GetPermitsToWorkQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PermitsToWorkQueryResult>> Handle(GetPermitsToWorkQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetPermitsToWorkAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<PermitsToWorkQueryResult>.Success(new PermitsToWorkQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve PermitsToWork for Tenant {TenantId}", request.TenantId);

            return Result<PermitsToWorkQueryResult>.Failure(new ApplicationError(
                "Operational.PermitsToWork.RetrievalFailed",
                "Unable to retrieve PermitsToWork data at this time.",
                Guid.NewGuid()));
        }
    }
}

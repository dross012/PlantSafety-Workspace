// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetPermitSchedulingQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.PermitToWork.PermitScheduling.Queries;
using PlantSafety.Application.PermitToWork.PermitScheduling.Repositories;

namespace PlantSafety.Application.PermitToWork.PermitScheduling.Handlers;

/// <summary>Handles <see cref="GetPermitSchedulingQuery"/>.</summary>
public sealed class GetPermitSchedulingQueryHandler : IRequestHandler<GetPermitSchedulingQuery, Result<PermitSchedulingQueryResult>>
{
    private readonly IPermitSchedulingRepository _repository;
    private readonly ILogger<GetPermitSchedulingQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPermitSchedulingQueryHandler(IPermitSchedulingRepository repository, ILogger<GetPermitSchedulingQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PermitSchedulingQueryResult>> Handle(GetPermitSchedulingQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetPermitSchedulingAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<PermitSchedulingQueryResult>.Success(new PermitSchedulingQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve PermitScheduling for Tenant {TenantId}", request.TenantId);

            return Result<PermitSchedulingQueryResult>.Failure(new ApplicationError(
                "PermitToWork.PermitScheduling.RetrievalFailed",
                "Unable to retrieve PermitScheduling data at this time.",
                Guid.NewGuid()));
        }
    }
}
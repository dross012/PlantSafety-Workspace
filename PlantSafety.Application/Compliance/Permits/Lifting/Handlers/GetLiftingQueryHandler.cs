// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetLiftingQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Permits.Lifting.Queries;
using PlantSafety.Application.Compliance.Permits.Lifting.Repositories;

namespace PlantSafety.Application.Compliance.Permits.Lifting.Handlers;

/// <summary>Handles <see cref="GetLiftingQuery"/>.</summary>
public sealed class GetLiftingQueryHandler : IRequestHandler<GetLiftingQuery, Result<LiftingQueryResult>>
{
    private readonly ILiftingRepository _repository;
    private readonly ILogger<GetLiftingQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetLiftingQueryHandler(ILiftingRepository repository, ILogger<GetLiftingQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<LiftingQueryResult>> Handle(GetLiftingQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetLiftingAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<LiftingQueryResult>.Success(new LiftingQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Lifting for Tenant {TenantId}", request.TenantId);

            return Result<LiftingQueryResult>.Failure(new ApplicationError(
                "Compliance.Permits.Lifting.RetrievalFailed",
                "Unable to retrieve lifting permit data at this time.",
                Guid.NewGuid()));
        }
    }
}

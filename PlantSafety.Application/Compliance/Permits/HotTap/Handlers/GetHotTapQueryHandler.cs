// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetHotTapQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Permits.HotTap.Queries;
using PlantSafety.Application.Compliance.Permits.HotTap.Repositories;

namespace PlantSafety.Application.Compliance.Permits.HotTap.Handlers;

/// <summary>Handles <see cref="GetHotTapQuery"/>.</summary>
public sealed class GetHotTapQueryHandler : IRequestHandler<GetHotTapQuery, Result<HotTapQueryResult>>
{
    private readonly IHotTapRepository _repository;
    private readonly ILogger<GetHotTapQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetHotTapQueryHandler(IHotTapRepository repository, ILogger<GetHotTapQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<HotTapQueryResult>> Handle(GetHotTapQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetHotTapAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<HotTapQueryResult>.Success(new HotTapQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve HotTap for Tenant {TenantId}", request.TenantId);

            return Result<HotTapQueryResult>.Failure(new ApplicationError(
                "Compliance.Permits.HotTap.RetrievalFailed",
                "Unable to retrieve hot tap permit data at this time.",
                Guid.NewGuid()));
        }
    }
}

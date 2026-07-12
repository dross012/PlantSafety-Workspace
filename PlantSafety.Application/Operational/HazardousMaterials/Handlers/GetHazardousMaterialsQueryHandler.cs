// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetHazardousMaterialsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.HazardousMaterials.Queries;
using PlantSafety.Application.Operational.HazardousMaterials.Repositories;

namespace PlantSafety.Application.Operational.HazardousMaterials.Handlers;

/// <summary>Handles <see cref="GetHazardousMaterialsQuery"/>.</summary>
public sealed class GetHazardousMaterialsQueryHandler : IRequestHandler<GetHazardousMaterialsQuery, Result<HazardousMaterialsQueryResult>>
{
    private readonly IHazardousMaterialsRepository _repository;
    private readonly ILogger<GetHazardousMaterialsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetHazardousMaterialsQueryHandler(IHazardousMaterialsRepository repository, ILogger<GetHazardousMaterialsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<HazardousMaterialsQueryResult>> Handle(GetHazardousMaterialsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetHazardousMaterialsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<HazardousMaterialsQueryResult>.Success(new HazardousMaterialsQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve HazardousMaterials for Tenant {TenantId}", request.TenantId);

            return Result<HazardousMaterialsQueryResult>.Failure(new ApplicationError(
                "Operational.HazardousMaterials.RetrievalFailed",
                "Unable to retrieve HazardousMaterials data at this time.",
                Guid.NewGuid()));
        }
    }
}

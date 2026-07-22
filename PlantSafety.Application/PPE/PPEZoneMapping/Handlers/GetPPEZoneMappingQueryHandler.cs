// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetPPEZoneMappingQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.PPE.PPEZoneMapping.Queries;
using PlantSafety.Application.PPE.PPEZoneMapping.Repositories;

namespace PlantSafety.Application.PPE.PPEZoneMapping.Handlers;

/// <summary>Handles <see cref="GetPPEZoneMappingQuery"/>.</summary>
public sealed class GetPPEZoneMappingQueryHandler : IRequestHandler<GetPPEZoneMappingQuery, Result<PPEZoneMappingQueryResult>>
{
    private readonly IPPEZoneMappingRepository _repository;
    private readonly ILogger<GetPPEZoneMappingQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPPEZoneMappingQueryHandler(IPPEZoneMappingRepository repository, ILogger<GetPPEZoneMappingQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PPEZoneMappingQueryResult>> Handle(GetPPEZoneMappingQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetPPEZoneMappingAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<PPEZoneMappingQueryResult>.Success(new PPEZoneMappingQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve PPEZoneMapping for Tenant {TenantId}", request.TenantId);

            return Result<PPEZoneMappingQueryResult>.Failure(new ApplicationError(
                "PPE.PPEZoneMapping.RetrievalFailed",
                "Unable to retrieve PPEZoneMapping data at this time.",
                Guid.NewGuid()));
        }
    }
}
// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for GetCarriersQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.LogisticsTransportSafety.Carriers.Queries;
using PlantSafety.Application.LogisticsTransportSafety.Carriers.Repositories;

namespace PlantSafety.Application.LogisticsTransportSafety.Carriers.Handlers;

/// <summary>Handles <see cref="GetCarriersQuery"/>.</summary>
public sealed class GetCarriersQueryHandler : IRequestHandler<GetCarriersQuery, Result<CarrierQueryResult>>
{
    private readonly ICarrierRepository _repository;
    private readonly ILogger<GetCarriersQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetCarriersQueryHandler(ICarrierRepository repository, ILogger<GetCarriersQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CarrierQueryResult>> Handle(GetCarriersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetCarrierItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<CarrierQueryResult>.Success(new CarrierQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Carrier records for Tenant {TenantId}", request.TenantId);

            return Result<CarrierQueryResult>.Failure(new ApplicationError(
                "LogisticsTransportSafety.Carriers.RetrievalFailed",
                "Unable to retrieve Carrier records at this time.",
                Guid.NewGuid()));
        }
    }
}

// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for GetTransportVehicleByIdQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.LogisticsTransportSafety.TransportVehicles.Queries;
using PlantSafety.Application.LogisticsTransportSafety.TransportVehicles.Repositories;

namespace PlantSafety.Application.LogisticsTransportSafety.TransportVehicles.Handlers;

/// <summary>Handles <see cref="GetTransportVehicleByIdQuery"/>.</summary>
public sealed class GetTransportVehicleByIdQueryHandler : IRequestHandler<GetTransportVehicleByIdQuery, Result<TransportVehicleQueryResult>>
{
    private readonly ITransportVehicleRepository _repository;
    private readonly ILogger<GetTransportVehicleByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetTransportVehicleByIdQueryHandler(ITransportVehicleRepository repository, ILogger<GetTransportVehicleByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<TransportVehicleQueryResult>> Handle(GetTransportVehicleByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetTransportVehicleByIdAsync(request.TenantId, request.TransportVehicleId, cancellationToken);

            return Result<TransportVehicleQueryResult>.Success(new TransportVehicleQueryResult(
                request.TenantId,
                DateTime.UtcNow,
                item));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve TransportVehicle {TransportVehicleId} for Tenant {TenantId}", request.TransportVehicleId, request.TenantId);

            return Result<TransportVehicleQueryResult>.Failure(new ApplicationError(
                "LogisticsTransportSafety.TransportVehicles.RetrievalFailed",
                "Unable to retrieve the TransportVehicle record at this time.",
                Guid.NewGuid()));
        }
    }
}

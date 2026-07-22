// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetEmergencyStopDevicesQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.MachineSafety.EmergencyStopDevices.Queries;
using PlantSafety.Application.MachineSafety.EmergencyStopDevices.Repositories;

namespace PlantSafety.Application.MachineSafety.EmergencyStopDevices.Handlers;

/// <summary>Handles <see cref="GetEmergencyStopDevicesQuery"/>.</summary>
public sealed class GetEmergencyStopDevicesQueryHandler : IRequestHandler<GetEmergencyStopDevicesQuery, Result<EmergencyStopDevicesQueryResult>>
{
    private readonly IEmergencyStopDevicesRepository _repository;
    private readonly ILogger<GetEmergencyStopDevicesQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetEmergencyStopDevicesQueryHandler(IEmergencyStopDevicesRepository repository, ILogger<GetEmergencyStopDevicesQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EmergencyStopDevicesQueryResult>> Handle(GetEmergencyStopDevicesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetEmergencyStopDevicesItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<EmergencyStopDevicesQueryResult>.Success(new EmergencyStopDevicesQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve EmergencyStopDevices records for Tenant {TenantId}", request.TenantId);

            return Result<EmergencyStopDevicesQueryResult>.Failure(new ApplicationError(
                "MachineSafety.EmergencyStopDevices.RetrievalFailed",
                "Unable to retrieve EmergencyStopDevices records at this time.",
                Guid.NewGuid()));
        }
    }
}
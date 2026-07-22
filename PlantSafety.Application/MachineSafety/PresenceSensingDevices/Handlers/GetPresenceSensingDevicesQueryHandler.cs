// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetPresenceSensingDevicesQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.MachineSafety.PresenceSensingDevices.Queries;
using PlantSafety.Application.MachineSafety.PresenceSensingDevices.Repositories;

namespace PlantSafety.Application.MachineSafety.PresenceSensingDevices.Handlers;

/// <summary>Handles <see cref="GetPresenceSensingDevicesQuery"/>.</summary>
public sealed class GetPresenceSensingDevicesQueryHandler : IRequestHandler<GetPresenceSensingDevicesQuery, Result<PresenceSensingDevicesQueryResult>>
{
    private readonly IPresenceSensingDevicesRepository _repository;
    private readonly ILogger<GetPresenceSensingDevicesQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPresenceSensingDevicesQueryHandler(IPresenceSensingDevicesRepository repository, ILogger<GetPresenceSensingDevicesQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PresenceSensingDevicesQueryResult>> Handle(GetPresenceSensingDevicesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetPresenceSensingDevicesItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<PresenceSensingDevicesQueryResult>.Success(new PresenceSensingDevicesQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve PresenceSensingDevices records for Tenant {TenantId}", request.TenantId);

            return Result<PresenceSensingDevicesQueryResult>.Failure(new ApplicationError(
                "MachineSafety.PresenceSensingDevices.RetrievalFailed",
                "Unable to retrieve PresenceSensingDevices records at this time.",
                Guid.NewGuid()));
        }
    }
}
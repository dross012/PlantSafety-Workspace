// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetEquipmentSafetyQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operational.EquipmentSafety.Queries;
using PlantSafety.Application.Operational.EquipmentSafety.Repositories;

namespace PlantSafety.Application.Operational.EquipmentSafety.Handlers;

/// <summary>Handles <see cref="GetEquipmentSafetyQuery"/>.</summary>
public sealed class GetEquipmentSafetyQueryHandler : IRequestHandler<GetEquipmentSafetyQuery, Result<EquipmentSafetyQueryResult>>
{
    private readonly IEquipmentSafetyRepository _repository;
    private readonly ILogger<GetEquipmentSafetyQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetEquipmentSafetyQueryHandler(IEquipmentSafetyRepository repository, ILogger<GetEquipmentSafetyQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EquipmentSafetyQueryResult>> Handle(GetEquipmentSafetyQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetEquipmentSafetyAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<EquipmentSafetyQueryResult>.Success(new EquipmentSafetyQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve EquipmentSafety for Tenant {TenantId}", request.TenantId);

            return Result<EquipmentSafetyQueryResult>.Failure(new ApplicationError(
                "Operational.EquipmentSafety.RetrievalFailed",
                "Unable to retrieve EquipmentSafety data at this time.",
                Guid.NewGuid()));
        }
    }
}

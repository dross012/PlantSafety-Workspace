// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetEquipmentQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Policy.Equipment.Queries;
using PlantSafety.Application.Compliance.Policy.Equipment.Repositories;

namespace PlantSafety.Application.Compliance.Policy.Equipment.Handlers;

/// <summary>Handles <see cref="GetEquipmentQuery"/>.</summary>
public sealed class GetEquipmentQueryHandler : IRequestHandler<GetEquipmentQuery, Result<EquipmentQueryResult>>
{
    private readonly IEquipmentRepository _repository;
    private readonly ILogger<GetEquipmentQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetEquipmentQueryHandler(IEquipmentRepository repository, ILogger<GetEquipmentQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EquipmentQueryResult>> Handle(GetEquipmentQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetEquipmentPoliciesAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<EquipmentQueryResult>.Success(new EquipmentQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Equipment for Tenant {TenantId}", request.TenantId);

            return Result<EquipmentQueryResult>.Failure(new ApplicationError(
                "Compliance.Policy.Equipment.RetrievalFailed",
                "Unable to retrieve Equipment data at this time.",
                Guid.NewGuid()));
        }
    }
}

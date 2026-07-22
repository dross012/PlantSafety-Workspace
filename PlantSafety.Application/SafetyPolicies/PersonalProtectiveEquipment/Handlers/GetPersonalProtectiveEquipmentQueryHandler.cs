// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetPersonalProtectiveEquipmentQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyPolicies.PersonalProtectiveEquipment.Queries;
using PlantSafety.Application.SafetyPolicies.PersonalProtectiveEquipment.Repositories;

namespace PlantSafety.Application.SafetyPolicies.PersonalProtectiveEquipment.Handlers;

/// <summary>Handles <see cref="GetPersonalProtectiveEquipmentQuery"/>.</summary>
public sealed class GetPersonalProtectiveEquipmentQueryHandler : IRequestHandler<GetPersonalProtectiveEquipmentQuery, Result<PersonalProtectiveEquipmentQueryResult>>
{
    private readonly IPersonalProtectiveEquipmentRepository _repository;
    private readonly ILogger<GetPersonalProtectiveEquipmentQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPersonalProtectiveEquipmentQueryHandler(IPersonalProtectiveEquipmentRepository repository, ILogger<GetPersonalProtectiveEquipmentQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PersonalProtectiveEquipmentQueryResult>> Handle(GetPersonalProtectiveEquipmentQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetPersonalProtectiveEquipmentAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<PersonalProtectiveEquipmentQueryResult>.Success(new PersonalProtectiveEquipmentQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve PersonalProtectiveEquipment for Tenant {TenantId}", request.TenantId);

            return Result<PersonalProtectiveEquipmentQueryResult>.Failure(new ApplicationError(
                "SafetyPolicies.PersonalProtectiveEquipment.RetrievalFailed",
                "Unable to retrieve PersonalProtectiveEquipment data at this time.",
                Guid.NewGuid()));
        }
    }
}
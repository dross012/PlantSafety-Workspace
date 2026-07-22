// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetPersonalProtectiveEquipmentQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Checklists.ChecklistTemplate.PersonalProtectiveEquipment.Queries;
using PlantSafety.Application.Checklists.ChecklistTemplate.PersonalProtectiveEquipment.Repositories;

namespace PlantSafety.Application.Checklists.ChecklistTemplate.PersonalProtectiveEquipment.Handlers;

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
            var items = await _repository.GetItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

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
                "Checklists.ChecklistTemplate.PersonalProtectiveEquipment.RetrievalFailed",
                "Unable to retrieve Personal Protective Equipment (PPE) Checklists data at this time.",
                Guid.NewGuid()));
        }
    }
}
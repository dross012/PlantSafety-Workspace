// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetEmergencySystemsAndLifeSafetyQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Checklists.ChecklistTemplate.EmergencySystemsAndLifeSafety.Queries;
using PlantSafety.Application.Checklists.ChecklistTemplate.EmergencySystemsAndLifeSafety.Repositories;

namespace PlantSafety.Application.Checklists.ChecklistTemplate.EmergencySystemsAndLifeSafety.Handlers;

/// <summary>Handles <see cref="GetEmergencySystemsAndLifeSafetyQuery"/>.</summary>
public sealed class GetEmergencySystemsAndLifeSafetyQueryHandler : IRequestHandler<GetEmergencySystemsAndLifeSafetyQuery, Result<EmergencySystemsAndLifeSafetyQueryResult>>
{
    private readonly IEmergencySystemsAndLifeSafetyRepository _repository;
    private readonly ILogger<GetEmergencySystemsAndLifeSafetyQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetEmergencySystemsAndLifeSafetyQueryHandler(IEmergencySystemsAndLifeSafetyRepository repository, ILogger<GetEmergencySystemsAndLifeSafetyQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EmergencySystemsAndLifeSafetyQueryResult>> Handle(GetEmergencySystemsAndLifeSafetyQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<EmergencySystemsAndLifeSafetyQueryResult>.Success(new EmergencySystemsAndLifeSafetyQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve EmergencySystemsAndLifeSafety for Tenant {TenantId}", request.TenantId);

            return Result<EmergencySystemsAndLifeSafetyQueryResult>.Failure(new ApplicationError(
                "Checklists.ChecklistTemplate.EmergencySystemsAndLifeSafety.RetrievalFailed",
                "Unable to retrieve Emergency Systems & Life Safety Checklists data at this time.",
                Guid.NewGuid()));
        }
    }
}
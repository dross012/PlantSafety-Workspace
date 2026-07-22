// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetHazardousMaterialsAndChemicalSafetyQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Checklists.ChecklistTemplate.HazardousMaterialsAndChemicalSafety.Queries;
using PlantSafety.Application.Checklists.ChecklistTemplate.HazardousMaterialsAndChemicalSafety.Repositories;

namespace PlantSafety.Application.Checklists.ChecklistTemplate.HazardousMaterialsAndChemicalSafety.Handlers;

/// <summary>Handles <see cref="GetHazardousMaterialsAndChemicalSafetyQuery"/>.</summary>
public sealed class GetHazardousMaterialsAndChemicalSafetyQueryHandler : IRequestHandler<GetHazardousMaterialsAndChemicalSafetyQuery, Result<HazardousMaterialsAndChemicalSafetyQueryResult>>
{
    private readonly IHazardousMaterialsAndChemicalSafetyRepository _repository;
    private readonly ILogger<GetHazardousMaterialsAndChemicalSafetyQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetHazardousMaterialsAndChemicalSafetyQueryHandler(IHazardousMaterialsAndChemicalSafetyRepository repository, ILogger<GetHazardousMaterialsAndChemicalSafetyQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<HazardousMaterialsAndChemicalSafetyQueryResult>> Handle(GetHazardousMaterialsAndChemicalSafetyQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<HazardousMaterialsAndChemicalSafetyQueryResult>.Success(new HazardousMaterialsAndChemicalSafetyQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve HazardousMaterialsAndChemicalSafety for Tenant {TenantId}", request.TenantId);

            return Result<HazardousMaterialsAndChemicalSafetyQueryResult>.Failure(new ApplicationError(
                "Checklists.ChecklistTemplate.HazardousMaterialsAndChemicalSafety.RetrievalFailed",
                "Unable to retrieve Hazardous Materials & Chemical Safety Checklists data at this time.",
                Guid.NewGuid()));
        }
    }
}
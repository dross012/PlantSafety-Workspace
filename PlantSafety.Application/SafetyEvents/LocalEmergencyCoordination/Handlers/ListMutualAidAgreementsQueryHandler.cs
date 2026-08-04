// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.LocalEmergencyCoordination
//  Layer:          Application
//  File:           ListMutualAidAgreementsQueryHandler.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Handler for ListMutualAidAgreementsQuery. Mirrors GetEmergencyManagementQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Queries;
using PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Repositories;

namespace PlantSafety.Application.SafetyEvents.LocalEmergencyCoordination.Handlers;

/// <summary>Handles <see cref="ListMutualAidAgreementsQuery"/>.</summary>
public sealed class ListMutualAidAgreementsQueryHandler : IRequestHandler<ListMutualAidAgreementsQuery, Result<ListMutualAidAgreementsResult>>
{
    private readonly ILocalEmergencyCoordinationRepository _repository;
    private readonly ILogger<ListMutualAidAgreementsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ListMutualAidAgreementsQueryHandler(ILocalEmergencyCoordinationRepository repository, ILogger<ListMutualAidAgreementsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ListMutualAidAgreementsResult>> Handle(ListMutualAidAgreementsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.ListMutualAidAgreementsAsync(request.TenantId, request.SiteId, cancellationToken);

            return Result<ListMutualAidAgreementsResult>.Success(new ListMutualAidAgreementsResult(
                request.TenantId, request.SiteId, DateTime.UtcNow, items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list mutual aid agreements for Tenant {TenantId}", request.TenantId);

            return Result<ListMutualAidAgreementsResult>.Failure(new ApplicationError(
                "SafetyEvents.LocalEmergencyCoordination.ListMutualAidAgreementsFailed",
                "Unable to retrieve mutual aid agreements at this time.",
                Guid.NewGuid()));
        }
    }
}

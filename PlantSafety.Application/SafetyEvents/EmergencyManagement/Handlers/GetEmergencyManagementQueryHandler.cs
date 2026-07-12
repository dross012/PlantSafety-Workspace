// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetEmergencyManagementQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyEvents.EmergencyManagement.Queries;
using PlantSafety.Application.SafetyEvents.EmergencyManagement.Repositories;

namespace PlantSafety.Application.SafetyEvents.EmergencyManagement.Handlers;

/// <summary>Handles <see cref="GetEmergencyManagementQuery"/>.</summary>
public sealed class GetEmergencyManagementQueryHandler : IRequestHandler<GetEmergencyManagementQuery, Result<EmergencyManagementQueryResult>>
{
    private readonly IEmergencyManagementRepository _repository;
    private readonly ILogger<GetEmergencyManagementQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetEmergencyManagementQueryHandler(IEmergencyManagementRepository repository, ILogger<GetEmergencyManagementQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EmergencyManagementQueryResult>> Handle(GetEmergencyManagementQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetEmergenciesAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<EmergencyManagementQueryResult>.Success(new EmergencyManagementQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve EmergencyManagement for Tenant {TenantId}", request.TenantId);

            return Result<EmergencyManagementQueryResult>.Failure(new ApplicationError(
                "SafetyEvents.EmergencyManagement.RetrievalFailed",
                "Unable to retrieve emergency management data at this time.",
                Guid.NewGuid()));
        }
    }
}

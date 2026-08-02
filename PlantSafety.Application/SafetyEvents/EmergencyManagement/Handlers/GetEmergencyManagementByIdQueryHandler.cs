// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.EmergencyManagement
//  Layer:          Application
//  File:           GetEmergencyManagementByIdQueryHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for GetEmergencyManagementByIdQuery. Reads from the read-side
//                  repository (optimised stored proc; no domain-aggregate reconstruction).
//                  Returns a NotFound failure both when the row does not exist and when it
//                  belongs to a different tenant, so tenant membership is never revealed to an
//                  unauthorized caller. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Handlers.GetPermitsByIdQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyEvents.EmergencyManagement.Models;
using PlantSafety.Application.SafetyEvents.EmergencyManagement.Queries;
using PlantSafety.Application.SafetyEvents.EmergencyManagement.Repositories;

namespace PlantSafety.Application.SafetyEvents.EmergencyManagement.Handlers;

/// <summary>Handles <see cref="GetEmergencyManagementByIdQuery"/>.</summary>
public sealed class GetEmergencyManagementByIdQueryHandler : IRequestHandler<GetEmergencyManagementByIdQuery, Result<EmergencyManagementItemDto>>
{
    private readonly IEmergencyManagementRepository _repository;
    private readonly ILogger<GetEmergencyManagementByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetEmergencyManagementByIdQueryHandler(IEmergencyManagementRepository repository, ILogger<GetEmergencyManagementByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EmergencyManagementItemDto>> Handle(GetEmergencyManagementByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.EmergencyManagementId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("EmergencyManagement {EmergencyManagementId} not found for Tenant {TenantId}", request.EmergencyManagementId, request.TenantId);

                return Result<EmergencyManagementItemDto>.Failure(new ApplicationError(
                    "SafetyEvents.EmergencyManagement.NotFound",
                    "The requested emergency could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<EmergencyManagementItemDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve EmergencyManagement {EmergencyManagementId}", request.EmergencyManagementId);

            return Result<EmergencyManagementItemDto>.Failure(new ApplicationError(
                "SafetyEvents.EmergencyManagement.RetrievalFailed",
                "Unable to retrieve emergency data at this time.",
                Guid.NewGuid()));
        }
    }
}

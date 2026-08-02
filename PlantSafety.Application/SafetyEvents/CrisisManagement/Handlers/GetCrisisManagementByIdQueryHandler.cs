// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SafetyEvents.CrisisManagement
//  Layer:          Application
//  File:           GetCrisisManagementByIdQueryHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for GetCrisisManagementByIdQuery. Reads from the read-side repository
//                  (optimised stored proc; no domain-aggregate reconstruction). Returns a NotFound
//                  failure both when the row does not exist and when it belongs to a different
//                  tenant, so tenant membership is never revealed to an unauthorized caller.
//                  tenantId is pushed into the repository call itself (usp_CrisisManagement_GetById's
//                  WHERE clause), not just checked here after the fact -- the post-fetch TenantId
//                  comparison below is defense-in-depth, not the primary control. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Handlers.GetPermitsByIdQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyEvents.CrisisManagement.Models;
using PlantSafety.Application.SafetyEvents.CrisisManagement.Queries;
using PlantSafety.Application.SafetyEvents.CrisisManagement.Repositories;

namespace PlantSafety.Application.SafetyEvents.CrisisManagement.Handlers;

/// <summary>Handles <see cref="GetCrisisManagementByIdQuery"/>.</summary>
public sealed class GetCrisisManagementByIdQueryHandler : IRequestHandler<GetCrisisManagementByIdQuery, Result<CrisisManagementDetailDto>>
{
    private readonly ICrisisManagementRepository _repository;
    private readonly ILogger<GetCrisisManagementByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetCrisisManagementByIdQueryHandler(ICrisisManagementRepository repository, ILogger<GetCrisisManagementByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CrisisManagementDetailDto>> Handle(GetCrisisManagementByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.CrisisManagementId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("CrisisManagement {CrisisManagementId} not found for Tenant {TenantId}", request.CrisisManagementId, request.TenantId);

                return Result<CrisisManagementDetailDto>.Failure(new ApplicationError(
                    "SafetyEvents.CrisisManagement.NotFound",
                    "The requested crisis could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<CrisisManagementDetailDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve CrisisManagement {CrisisManagementId}", request.CrisisManagementId);

            return Result<CrisisManagementDetailDto>.Failure(new ApplicationError(
                "SafetyEvents.CrisisManagement.RetrievalFailed",
                "Unable to retrieve crisis data at this time.",
                Guid.NewGuid()));
        }
    }
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Governance
//  Layer:          Application
//  File:           GetGovernanceByIdQueryHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for GetGovernanceByIdQuery. Reads from the read-side repository.
//                  Returns a NotFound failure both when the row does not exist and when it
//                  belongs to a different tenant, so tenant membership is never revealed to an
//                  unauthorized caller. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Handlers.GetAuditingByIdQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Governance.Models;
using PlantSafety.Application.Governance.Queries;
using PlantSafety.Application.Governance.Repositories;

namespace PlantSafety.Application.Governance.Handlers;

/// <summary>Handles <see cref="GetGovernanceByIdQuery"/>.</summary>
public sealed class GetGovernanceByIdQueryHandler : IRequestHandler<GetGovernanceByIdQuery, Result<GovernanceItemDto>>
{
    private readonly IGovernanceRepository _repository;
    private readonly ILogger<GetGovernanceByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetGovernanceByIdQueryHandler(IGovernanceRepository repository, ILogger<GetGovernanceByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<GovernanceItemDto>> Handle(GetGovernanceByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.GovernanceId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("Governance {GovernanceId} not found for Tenant {TenantId}", request.GovernanceId, request.TenantId);

                return Result<GovernanceItemDto>.Failure(new ApplicationError(
                    "Governance.NotFound",
                    "The requested governance record could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<GovernanceItemDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Governance {GovernanceId}", request.GovernanceId);

            return Result<GovernanceItemDto>.Failure(new ApplicationError(
                "Governance.RetrievalFailed",
                "Unable to retrieve governance data at this time.",
                Guid.NewGuid()));
        }
    }
}

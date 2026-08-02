// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AiGovernance
//  Layer:          Application
//  File:           GetAiGovernanceByIdQueryHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for GetAiGovernanceByIdQuery. Reads from the read-side repository.
//                  Returns a NotFound failure both when the row does not exist and when it belongs
//                  to a different tenant, so tenant membership is never revealed to an unauthorized
//                  caller. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Handlers.GetAuditingByIdQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.AiGovernance.Models;
using PlantSafety.Application.AiGovernance.Queries;
using PlantSafety.Application.AiGovernance.Repositories;

namespace PlantSafety.Application.AiGovernance.Handlers;

/// <summary>Handles <see cref="GetAiGovernanceByIdQuery"/>.</summary>
public sealed class GetAiGovernanceByIdQueryHandler : IRequestHandler<GetAiGovernanceByIdQuery, Result<AiGovernanceItemDto>>
{
    private readonly IAiGovernanceRepository _repository;
    private readonly ILogger<GetAiGovernanceByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetAiGovernanceByIdQueryHandler(IAiGovernanceRepository repository, ILogger<GetAiGovernanceByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AiGovernanceItemDto>> Handle(GetAiGovernanceByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.AiGovernanceId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("AiGovernance {AiGovernanceId} not found for Tenant {TenantId}", request.AiGovernanceId, request.TenantId);

                return Result<AiGovernanceItemDto>.Failure(new ApplicationError(
                    "AiGovernance.NotFound",
                    "The requested AI governance record could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<AiGovernanceItemDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve AiGovernance {AiGovernanceId}", request.AiGovernanceId);

            return Result<AiGovernanceItemDto>.Failure(new ApplicationError(
                "AiGovernance.RetrievalFailed",
                "Unable to retrieve AI governance data at this time.",
                Guid.NewGuid()));
        }
    }
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         AiSafetyIntelligence
//  Layer:          Application
//  File:           GetAiSafetyIntelligenceByIdQueryHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for GetAiSafetyIntelligenceByIdQuery. Reads from the read-side
//                  repository. Returns a NotFound failure both when the row does not exist and
//                  when it belongs to a different tenant, so tenant membership is never revealed
//                  to an unauthorized caller. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.AiSafetyIntelligence.Models;
using PlantSafety.Application.AiSafetyIntelligence.Queries;
using PlantSafety.Application.AiSafetyIntelligence.Repositories;

namespace PlantSafety.Application.AiSafetyIntelligence.Handlers;

/// <summary>Handles <see cref="GetAiSafetyIntelligenceByIdQuery"/>.</summary>
public sealed class GetAiSafetyIntelligenceByIdQueryHandler : IRequestHandler<GetAiSafetyIntelligenceByIdQuery, Result<AiSafetyIntelligenceItemDto>>
{
    private readonly IAiSafetyIntelligenceRepository _repository;
    private readonly ILogger<GetAiSafetyIntelligenceByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetAiSafetyIntelligenceByIdQueryHandler(IAiSafetyIntelligenceRepository repository, ILogger<GetAiSafetyIntelligenceByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AiSafetyIntelligenceItemDto>> Handle(GetAiSafetyIntelligenceByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.AiSafetyIntelligenceId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("AiSafetyIntelligence {AiSafetyIntelligenceId} not found for Tenant {TenantId}", request.AiSafetyIntelligenceId, request.TenantId);

                return Result<AiSafetyIntelligenceItemDto>.Failure(new ApplicationError(
                    "AiSafetyIntelligence.NotFound",
                    "The requested AI safety intelligence record could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<AiSafetyIntelligenceItemDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve AiSafetyIntelligence {AiSafetyIntelligenceId}", request.AiSafetyIntelligenceId);

            return Result<AiSafetyIntelligenceItemDto>.Failure(new ApplicationError(
                "AiSafetyIntelligence.RetrievalFailed",
                "Unable to retrieve AI safety intelligence data at this time.",
                Guid.NewGuid()));
        }
    }
}

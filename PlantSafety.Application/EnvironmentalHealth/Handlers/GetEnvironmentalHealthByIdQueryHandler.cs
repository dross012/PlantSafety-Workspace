// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnvironmentalHealth
//  Layer:          Application
//  File:           GetEnvironmentalHealthByIdQueryHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for GetEnvironmentalHealthByIdQuery. Reads from the read-side
//                  repository. Returns a NotFound failure both when the row does not exist and
//                  when it belongs to a different tenant, so tenant membership is never
//                  revealed to an unauthorized caller. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Handlers.GetAuditingByIdQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.EnvironmentalHealth.Models;
using PlantSafety.Application.EnvironmentalHealth.Queries;
using PlantSafety.Application.EnvironmentalHealth.Repositories;

namespace PlantSafety.Application.EnvironmentalHealth.Handlers;

/// <summary>Handles <see cref="GetEnvironmentalHealthByIdQuery"/>.</summary>
public sealed class GetEnvironmentalHealthByIdQueryHandler : IRequestHandler<GetEnvironmentalHealthByIdQuery, Result<EnvironmentalHealthItemDto>>
{
    private readonly IEnvironmentalHealthRepository _repository;
    private readonly ILogger<GetEnvironmentalHealthByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetEnvironmentalHealthByIdQueryHandler(IEnvironmentalHealthRepository repository, ILogger<GetEnvironmentalHealthByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EnvironmentalHealthItemDto>> Handle(GetEnvironmentalHealthByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.EnvironmentalHealthId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("EnvironmentalHealth {EnvironmentalHealthId} not found for Tenant {TenantId}", request.EnvironmentalHealthId, request.TenantId);

                return Result<EnvironmentalHealthItemDto>.Failure(new ApplicationError(
                    "EnvironmentalHealth.NotFound",
                    "The requested environmental-health record could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<EnvironmentalHealthItemDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve EnvironmentalHealth {EnvironmentalHealthId}", request.EnvironmentalHealthId);

            return Result<EnvironmentalHealthItemDto>.Failure(new ApplicationError(
                "EnvironmentalHealth.RetrievalFailed",
                "Unable to retrieve environmental-health data at this time.",
                Guid.NewGuid()));
        }
    }
}

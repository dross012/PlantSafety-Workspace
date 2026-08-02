// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Sustainability
//  Layer:          Application
//  File:           GetSustainabilityByIdQueryHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for GetSustainabilityByIdQuery. Reads from the read-side repository.
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
using PlantSafety.Application.Sustainability.Models;
using PlantSafety.Application.Sustainability.Queries;
using PlantSafety.Application.Sustainability.Repositories;

namespace PlantSafety.Application.Sustainability.Handlers;

/// <summary>Handles <see cref="GetSustainabilityByIdQuery"/>.</summary>
public sealed class GetSustainabilityByIdQueryHandler : IRequestHandler<GetSustainabilityByIdQuery, Result<SustainabilityItemDto>>
{
    private readonly ISustainabilityRepository _repository;
    private readonly ILogger<GetSustainabilityByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetSustainabilityByIdQueryHandler(ISustainabilityRepository repository, ILogger<GetSustainabilityByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<SustainabilityItemDto>> Handle(GetSustainabilityByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.SustainabilityId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("Sustainability {SustainabilityId} not found for Tenant {TenantId}", request.SustainabilityId, request.TenantId);

                return Result<SustainabilityItemDto>.Failure(new ApplicationError(
                    "Sustainability.Sustainability.NotFound",
                    "The requested sustainability initiative could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<SustainabilityItemDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Sustainability {SustainabilityId}", request.SustainabilityId);

            return Result<SustainabilityItemDto>.Failure(new ApplicationError(
                "Sustainability.Sustainability.RetrievalFailed",
                "Unable to retrieve sustainability initiative data at this time.",
                Guid.NewGuid()));
        }
    }
}

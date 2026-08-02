// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         DigitalTwin
//  Layer:          Application
//  File:           GetDigitalTwinByIdQueryHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for GetDigitalTwinByIdQuery. Reads from the read-side repository.
//                  Returns a NotFound failure both when the row does not exist and when it belongs
//                  to a different tenant, so tenant membership is never revealed to an unauthorized
//                  caller. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.DigitalTwin.Models;
using PlantSafety.Application.DigitalTwin.Queries;
using PlantSafety.Application.DigitalTwin.Repositories;

namespace PlantSafety.Application.DigitalTwin.Handlers;

/// <summary>Handles <see cref="GetDigitalTwinByIdQuery"/>.</summary>
public sealed class GetDigitalTwinByIdQueryHandler : IRequestHandler<GetDigitalTwinByIdQuery, Result<DigitalTwinItemDto>>
{
    private readonly IDigitalTwinRepository _repository;
    private readonly ILogger<GetDigitalTwinByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetDigitalTwinByIdQueryHandler(IDigitalTwinRepository repository, ILogger<GetDigitalTwinByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<DigitalTwinItemDto>> Handle(GetDigitalTwinByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.DigitalTwinId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("DigitalTwin {DigitalTwinId} not found for Tenant {TenantId}", request.DigitalTwinId, request.TenantId);

                return Result<DigitalTwinItemDto>.Failure(new ApplicationError(
                    "DigitalTwin.NotFound",
                    "The requested digital-twin record could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<DigitalTwinItemDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve DigitalTwin {DigitalTwinId}", request.DigitalTwinId);

            return Result<DigitalTwinItemDto>.Failure(new ApplicationError(
                "DigitalTwin.RetrievalFailed",
                "Unable to retrieve digital-twin data at this time.",
                Guid.NewGuid()));
        }
    }
}

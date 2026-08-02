// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         EnterpriseFabric
//  Layer:          Application
//  File:           GetEnterpriseFabricByIdQueryHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for GetEnterpriseFabricByIdQuery. Reads from the read-side repository.
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
using PlantSafety.Application.EnterpriseFabric.Models;
using PlantSafety.Application.EnterpriseFabric.Queries;
using PlantSafety.Application.EnterpriseFabric.Repositories;

namespace PlantSafety.Application.EnterpriseFabric.Handlers;

/// <summary>Handles <see cref="GetEnterpriseFabricByIdQuery"/>.</summary>
public sealed class GetEnterpriseFabricByIdQueryHandler : IRequestHandler<GetEnterpriseFabricByIdQuery, Result<EnterpriseFabricItemDto>>
{
    private readonly IEnterpriseFabricRepository _repository;
    private readonly ILogger<GetEnterpriseFabricByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetEnterpriseFabricByIdQueryHandler(IEnterpriseFabricRepository repository, ILogger<GetEnterpriseFabricByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EnterpriseFabricItemDto>> Handle(GetEnterpriseFabricByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.EnterpriseFabricId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("EnterpriseFabric {EnterpriseFabricId} not found for Tenant {TenantId}", request.EnterpriseFabricId, request.TenantId);

                return Result<EnterpriseFabricItemDto>.Failure(new ApplicationError(
                    "EnterpriseFabric.NotFound",
                    "The requested enterprise fabric record could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<EnterpriseFabricItemDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve EnterpriseFabric {EnterpriseFabricId}", request.EnterpriseFabricId);

            return Result<EnterpriseFabricItemDto>.Failure(new ApplicationError(
                "EnterpriseFabric.RetrievalFailed",
                "Unable to retrieve enterprise fabric data at this time.",
                Guid.NewGuid()));
        }
    }
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         OperationalSafety
//  Layer:          Application
//  File:           GetOperationalSafetyByIdQueryHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for GetOperationalSafetyByIdQuery. Reads from the read-side
//                  repository. Returns a NotFound failure both when the row does not exist and
//                  when it belongs to a different tenant, so tenant membership is never revealed
//                  to an unauthorized caller. Mirrors
//                  PlantSafety.Application.Compliance.Auditing.Handlers.GetAuditingByIdQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.OperationalSafety.Models;
using PlantSafety.Application.OperationalSafety.Queries;
using PlantSafety.Application.OperationalSafety.Repositories;

namespace PlantSafety.Application.OperationalSafety.Handlers;

/// <summary>Handles <see cref="GetOperationalSafetyByIdQuery"/>.</summary>
public sealed class GetOperationalSafetyByIdQueryHandler : IRequestHandler<GetOperationalSafetyByIdQuery, Result<OperationalSafetyItemDto>>
{
    private readonly IOperationalSafetyRepository _repository;
    private readonly ILogger<GetOperationalSafetyByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetOperationalSafetyByIdQueryHandler(IOperationalSafetyRepository repository, ILogger<GetOperationalSafetyByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<OperationalSafetyItemDto>> Handle(GetOperationalSafetyByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.OperationalSafetyId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("OperationalSafety {OperationalSafetyId} not found for Tenant {TenantId}", request.OperationalSafetyId, request.TenantId);

                return Result<OperationalSafetyItemDto>.Failure(new ApplicationError(
                    "OperationalSafety.NotFound",
                    "The requested operational-safety record could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<OperationalSafetyItemDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve OperationalSafety {OperationalSafetyId}", request.OperationalSafetyId);

            return Result<OperationalSafetyItemDto>.Failure(new ApplicationError(
                "OperationalSafety.RetrievalFailed",
                "Unable to retrieve operational-safety data at this time.",
                Guid.NewGuid()));
        }
    }
}

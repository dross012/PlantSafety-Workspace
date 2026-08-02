// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Auditing
//  Layer:          Application
//  File:           GetAuditingByIdQueryHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for GetAuditingByIdQuery. Reads from the read-side repository. Returns
//                  a NotFound failure both when the row does not exist and when it belongs to a
//                  different tenant, so tenant membership is never revealed to an unauthorized
//                  caller. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Handlers.GetPermitsByIdQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Auditing.Models;
using PlantSafety.Application.Compliance.Auditing.Queries;
using PlantSafety.Application.Compliance.Auditing.Repositories;

namespace PlantSafety.Application.Compliance.Auditing.Handlers;

/// <summary>Handles <see cref="GetAuditingByIdQuery"/>.</summary>
public sealed class GetAuditingByIdQueryHandler : IRequestHandler<GetAuditingByIdQuery, Result<AuditingItemDto>>
{
    private readonly IAuditingRepository _repository;
    private readonly ILogger<GetAuditingByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetAuditingByIdQueryHandler(IAuditingRepository repository, ILogger<GetAuditingByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AuditingItemDto>> Handle(GetAuditingByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.AuditingId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("Auditing {AuditingId} not found for Tenant {TenantId}", request.AuditingId, request.TenantId);

                return Result<AuditingItemDto>.Failure(new ApplicationError(
                    "Compliance.Auditing.NotFound",
                    "The requested audit record could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<AuditingItemDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Auditing {AuditingId}", request.AuditingId);

            return Result<AuditingItemDto>.Failure(new ApplicationError(
                "Compliance.Auditing.RetrievalFailed",
                "Unable to retrieve audit data at this time.",
                Guid.NewGuid()));
        }
    }
}

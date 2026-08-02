// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Compliance
//  Layer:          Application
//  File:           GetComplianceByIdQueryHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for GetComplianceByIdQuery. Reads from the read-side repository.
//                  Returns a NotFound failure both when the row does not exist and when it
//                  belongs to a different tenant, so tenant membership is never revealed to an
//                  unauthorized caller. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Handlers.GetPermitsByIdQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Compliance.Models;
using PlantSafety.Application.Compliance.Compliance.Queries;
using PlantSafety.Application.Compliance.Compliance.Repositories;

namespace PlantSafety.Application.Compliance.Compliance.Handlers;

/// <summary>Handles <see cref="GetComplianceByIdQuery"/>.</summary>
public sealed class GetComplianceByIdQueryHandler : IRequestHandler<GetComplianceByIdQuery, Result<ComplianceItemDto>>
{
    private readonly IComplianceRepository _repository;
    private readonly ILogger<GetComplianceByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetComplianceByIdQueryHandler(IComplianceRepository repository, ILogger<GetComplianceByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ComplianceItemDto>> Handle(GetComplianceByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.ComplianceId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("Compliance {ComplianceId} not found for Tenant {TenantId}", request.ComplianceId, request.TenantId);

                return Result<ComplianceItemDto>.Failure(new ApplicationError(
                    "Compliance.Compliance.NotFound",
                    "The requested compliance record could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<ComplianceItemDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Compliance {ComplianceId}", request.ComplianceId);

            return Result<ComplianceItemDto>.Failure(new ApplicationError(
                "Compliance.Compliance.RetrievalFailed",
                "Unable to retrieve compliance data at this time.",
                Guid.NewGuid()));
        }
    }
}

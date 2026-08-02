// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Certification
//  Layer:          Application
//  File:           GetCertificationByIdQueryHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for GetCertificationByIdQuery. Reads from the read-side repository.
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
using PlantSafety.Application.Compliance.Certification.Models;
using PlantSafety.Application.Compliance.Certification.Queries;
using PlantSafety.Application.Compliance.Certification.Repositories;

namespace PlantSafety.Application.Compliance.Certification.Handlers;

/// <summary>Handles <see cref="GetCertificationByIdQuery"/>.</summary>
public sealed class GetCertificationByIdQueryHandler : IRequestHandler<GetCertificationByIdQuery, Result<CertificationItemDto>>
{
    private readonly ICertificationRepository _repository;
    private readonly ILogger<GetCertificationByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetCertificationByIdQueryHandler(ICertificationRepository repository, ILogger<GetCertificationByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CertificationItemDto>> Handle(GetCertificationByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.CertificationId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("Certification {CertificationId} not found for Tenant {TenantId}", request.CertificationId, request.TenantId);

                return Result<CertificationItemDto>.Failure(new ApplicationError(
                    "Compliance.Certification.NotFound",
                    "The requested certification record could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<CertificationItemDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Certification {CertificationId}", request.CertificationId);

            return Result<CertificationItemDto>.Failure(new ApplicationError(
                "Compliance.Certification.RetrievalFailed",
                "Unable to retrieve certification data at this time.",
                Guid.NewGuid()));
        }
    }
}

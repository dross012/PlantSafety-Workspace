// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Incidents
//  Layer:          Application
//  File:           GetIncidentReportQueryHandler.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Handler for GetIncidentReportQuery. Reads from the read-side repository
//                  (optimised stored proc; no domain-aggregate reconstruction). Returns a
//                  NotFound failure both when the row does not exist and when it belongs to a
//                  different tenant, so tenant membership is never revealed to an unauthorized
//                  caller. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Handlers.GetPermitsByIdQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.PHMSA.Incidents.Models;
using PlantSafety.Application.Compliance.PHMSA.Incidents.Queries;
using PlantSafety.Application.Compliance.PHMSA.Incidents.Repositories;

namespace PlantSafety.Application.Compliance.PHMSA.Incidents.Handlers;

/// <summary>Handles <see cref="GetIncidentReportQuery"/>.</summary>
public sealed class GetIncidentReportQueryHandler : IRequestHandler<GetIncidentReportQuery, Result<IncidentReportDto>>
{
    private readonly IIncidentReportRepository _repository;
    private readonly ILogger<GetIncidentReportQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetIncidentReportQueryHandler(IIncidentReportRepository repository, ILogger<GetIncidentReportQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IncidentReportDto>> Handle(GetIncidentReportQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.IncidentReportId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("IncidentReport {IncidentReportId} not found for Tenant {TenantId}", request.IncidentReportId, request.TenantId);

                return Result<IncidentReportDto>.Failure(new ApplicationError(
                    "Compliance.PHMSA.Incidents.NotFound",
                    "The requested incident report could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<IncidentReportDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve IncidentReport {IncidentReportId}", request.IncidentReportId);

            return Result<IncidentReportDto>.Failure(new ApplicationError(
                "Compliance.PHMSA.Incidents.RetrievalFailed",
                "Unable to retrieve incident report data at this time.",
                Guid.NewGuid()));
        }
    }
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Incidents
//  Layer:          Application
//  File:           ListIncidentReportsQueryHandler.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Handler for ListIncidentReportsQuery. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Handlers.GetPermitsQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.PHMSA.Incidents.Queries;
using PlantSafety.Application.Compliance.PHMSA.Incidents.Repositories;

namespace PlantSafety.Application.Compliance.PHMSA.Incidents.Handlers;

/// <summary>Handles <see cref="ListIncidentReportsQuery"/>.</summary>
public sealed class ListIncidentReportsQueryHandler : IRequestHandler<ListIncidentReportsQuery, Result<ListIncidentReportsResult>>
{
    private readonly IIncidentReportRepository _repository;
    private readonly ILogger<ListIncidentReportsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ListIncidentReportsQueryHandler(IIncidentReportRepository repository, ILogger<ListIncidentReportsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ListIncidentReportsResult>> Handle(ListIncidentReportsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.ListAsync(
                request.TenantId, request.SiteId, request.FromUtc, request.ToUtc, request.IsReportableToPhmsa, cancellationToken);

            return Result<ListIncidentReportsResult>.Success(new ListIncidentReportsResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list IncidentReports for Tenant {TenantId}", request.TenantId);

            return Result<ListIncidentReportsResult>.Failure(new ApplicationError(
                "Compliance.PHMSA.Incidents.RetrievalFailed",
                "Unable to retrieve incident report data at this time.",
                Guid.NewGuid()));
        }
    }
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RegulatoryCalendar
//  Layer:          Application
//  File:           GetRegulatoryCalendarQueryHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for GetRegulatoryCalendarQuery. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Handlers.GetPermitsQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.RegulatoryCalendar.Queries;
using PlantSafety.Application.Compliance.RegulatoryCalendar.Repositories;

namespace PlantSafety.Application.Compliance.RegulatoryCalendar.Handlers;

/// <summary>Handles <see cref="GetRegulatoryCalendarQuery"/>.</summary>
public sealed class GetRegulatoryCalendarQueryHandler : IRequestHandler<GetRegulatoryCalendarQuery, Result<RegulatoryCalendarQueryResult>>
{
    private readonly IRegulatoryCalendarRepository _repository;
    private readonly ILogger<GetRegulatoryCalendarQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetRegulatoryCalendarQueryHandler(IRegulatoryCalendarRepository repository, ILogger<GetRegulatoryCalendarQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RegulatoryCalendarQueryResult>> Handle(GetRegulatoryCalendarQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetRegulatoryCalendarAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<RegulatoryCalendarQueryResult>.Success(new RegulatoryCalendarQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve RegulatoryCalendar for Tenant {TenantId}", request.TenantId);

            return Result<RegulatoryCalendarQueryResult>.Failure(new ApplicationError(
                "Compliance.RegulatoryCalendar.RetrievalFailed",
                "Unable to retrieve RegulatoryCalendar data at this time.",
                Guid.NewGuid()));
        }
    }
}

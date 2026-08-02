// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RegulatoryCalendar
//  Layer:          Application
//  File:           GetRegulatoryCalendarByIdQueryHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for GetRegulatoryCalendarByIdQuery. Reads from the read-side repository
//                  (optimised stored proc; no domain-aggregate reconstruction). Returns a NotFound
//                  failure both when the row does not exist and when it belongs to a different
//                  tenant, so tenant membership is never revealed to an unauthorized caller.
//                  tenantId is pushed into the repository call itself
//                  (usp_RegulatoryCalendar_GetById's WHERE clause), not just checked here after
//                  the fact -- the post-fetch TenantId comparison below is defense-in-depth, not
//                  the primary control. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Handlers.GetPermitsByIdQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.RegulatoryCalendar.Models;
using PlantSafety.Application.Compliance.RegulatoryCalendar.Queries;
using PlantSafety.Application.Compliance.RegulatoryCalendar.Repositories;

namespace PlantSafety.Application.Compliance.RegulatoryCalendar.Handlers;

/// <summary>Handles <see cref="GetRegulatoryCalendarByIdQuery"/>.</summary>
public sealed class GetRegulatoryCalendarByIdQueryHandler : IRequestHandler<GetRegulatoryCalendarByIdQuery, Result<RegulatoryCalendarItemDto>>
{
    private readonly IRegulatoryCalendarRepository _repository;
    private readonly ILogger<GetRegulatoryCalendarByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetRegulatoryCalendarByIdQueryHandler(IRegulatoryCalendarRepository repository, ILogger<GetRegulatoryCalendarByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<RegulatoryCalendarItemDto>> Handle(GetRegulatoryCalendarByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetByIdAsync(request.RegulatoryCalendarId, request.TenantId, cancellationToken);

            if (item is null || item.TenantId != request.TenantId)
            {
                _logger.LogDebug("RegulatoryCalendar {RegulatoryCalendarId} not found for Tenant {TenantId}", request.RegulatoryCalendarId, request.TenantId);

                return Result<RegulatoryCalendarItemDto>.Failure(new ApplicationError(
                    "Compliance.RegulatoryCalendar.NotFound",
                    "The requested regulatory calendar record could not be found.",
                    Guid.NewGuid(),
                    ErrorSeverity.NotFound));
            }

            return Result<RegulatoryCalendarItemDto>.Success(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve RegulatoryCalendar {RegulatoryCalendarId}", request.RegulatoryCalendarId);

            return Result<RegulatoryCalendarItemDto>.Failure(new ApplicationError(
                "Compliance.RegulatoryCalendar.RetrievalFailed",
                "Unable to retrieve regulatory calendar data at this time.",
                Guid.NewGuid()));
        }
    }
}

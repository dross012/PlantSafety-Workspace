// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         GlobalSafetyInsights
//  Layer:          Application
//  File:           GetCrossSiteActionHistoryQueryHandler.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 8 §5 Global Safety Dashboard)
//  SchemaVersion:  2026.1
//  Description:    Handles GetCrossSiteActionHistoryQuery. Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.GlobalSafetyInsights.Models;
using PlantSafety.Application.GlobalSafetyInsights.Queries;
using PlantSafety.Application.GlobalSafetyInsights.Repositories;

namespace PlantSafety.Application.GlobalSafetyInsights.Handlers;

/// <summary>Handles <see cref="GetCrossSiteActionHistoryQuery"/>.</summary>
public sealed class GetCrossSiteActionHistoryQueryHandler
    : IRequestHandler<GetCrossSiteActionHistoryQuery, Result<IReadOnlyList<CrossSiteActionLogEntryDto>>>
{
    private readonly IGlobalSafetyActionAuditRepository _repository;
    private readonly ILogger<GetCrossSiteActionHistoryQueryHandler> _logger;

    /// <summary>Initializes dependencies.</summary>
    public GetCrossSiteActionHistoryQueryHandler(
        IGlobalSafetyActionAuditRepository repository, ILogger<GetCrossSiteActionHistoryQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<CrossSiteActionLogEntryDto>>> Handle(GetCrossSiteActionHistoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var entries = await _repository.ListAsync(request.TenantId, request.From, request.To, cancellationToken);
            return Result<IReadOnlyList<CrossSiteActionLogEntryDto>>.Success(entries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve CrossSiteAction history for Tenant {TenantId}", request.TenantId);

            return Result<IReadOnlyList<CrossSiteActionLogEntryDto>>.Failure(new ApplicationError(
                "GlobalSafetyInsights.CrossSiteActionHistoryRetrievalFailed",
                "Unable to retrieve cross-site action history at this time.",
                Guid.NewGuid()));
        }
    }
}

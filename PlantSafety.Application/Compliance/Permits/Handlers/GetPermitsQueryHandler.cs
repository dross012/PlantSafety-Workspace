// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.Permits
//  Layer:          Application
//  File:           GetPermitsQueryHandler.cs
//  Created:        2026-07-30
//  SchemaVersion:  2026.1
//  Description:    Handler for GetPermitsQuery. Mirrors
//                  PlantSafety.Application.PermitToWork.PermitWorkflow.Handlers.GetPermitWorkflowQueryHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Permits.Queries;
using PlantSafety.Application.Compliance.Permits.Repositories;

namespace PlantSafety.Application.Compliance.Permits.Handlers;

/// <summary>Handles <see cref="GetPermitsQuery"/>.</summary>
public sealed class GetPermitsQueryHandler : IRequestHandler<GetPermitsQuery, Result<PermitsQueryResult>>
{
    private readonly IPermitsRepository _repository;
    private readonly ILogger<GetPermitsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPermitsQueryHandler(IPermitsRepository repository, ILogger<GetPermitsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PermitsQueryResult>> Handle(GetPermitsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetPermitsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<PermitsQueryResult>.Success(new PermitsQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Permits for Tenant {TenantId}", request.TenantId);

            return Result<PermitsQueryResult>.Failure(new ApplicationError(
                "Compliance.Permits.RetrievalFailed",
                "Unable to retrieve Permits data at this time.",
                Guid.NewGuid()));
        }
    }
}

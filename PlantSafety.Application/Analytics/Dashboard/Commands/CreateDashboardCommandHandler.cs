// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Dashboard
//  Layer:          Application
//  File:           CreateDashboardCommandHandler.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Handler for CreateDashboardCommand. Pure orchestration: constructs the
//                  Dashboard aggregate via its domain factory method and persists it through the
//                  domain repository interface. No business rules live here. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CreatePermitsCommandHandler.
//                  Additive-only evolution.
// ============================================================================
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.Analytics.Dashboard.Interfaces;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Analytics.Dashboard.Commands;

/// <summary>Handles <see cref="CreateDashboardCommand"/>.</summary>
public sealed class CreateDashboardCommandHandler : IRequestHandler<CreateDashboardCommand, Result<CreateDashboardResult>>
{
    private readonly IDashboardRepository _repository;
    private readonly ILogger<CreateDashboardCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public CreateDashboardCommandHandler(IDashboardRepository repository, ILogger<CreateDashboardCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CreateDashboardResult>> Handle(CreateDashboardCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var requesterId = UserId.FromGuid(request.RequesterUserId);
            var ownerId = UserId.FromGuid(request.OwnerUserId);

            var aggregate = PlantSafety.Domain.Analytics.Dashboard.Aggregates.Dashboard.Create(
                createdBy: requesterId,
                createdAtUtc: UtcTimestamp.Now(),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId),
                actor: PerformedByActor.User(requesterId),
                tenantId: request.TenantId,
                siteId: request.SiteId,
                dashboardName: request.DashboardName,
                dashboardType: request.DashboardType,
                ownerId: ownerId,
                refreshIntervalSeconds: request.RefreshIntervalSeconds,
                widgetCount: request.WidgetCount);

            await _repository.AddAsync(aggregate, cancellationToken);

            return Result<CreateDashboardResult>.Success(new CreateDashboardResult(
                aggregate.DashboardId.Value,
                aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Dashboard for Tenant {TenantId}", request.TenantId);

            return Result<CreateDashboardResult>.Failure(new ApplicationError(
                "Analytics.Dashboard.CreateFailed",
                "Unable to define the dashboard at this time.",
                Guid.NewGuid()));
        }
    }
}

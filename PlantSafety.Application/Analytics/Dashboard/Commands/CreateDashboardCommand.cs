// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Dashboard
//  Layer:          Application
//  File:           CreateDashboardCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to define a new KPI Dashboard. Carries the full command envelope
//                  (CorrelationId, CausationId, CommandId, RequestedAtUtc, ApiVersionToken) per
//                  Handbook Rule 2.1. Phase 4 full build-out -- this aggregate previously had no
//                  Application-layer commands at all. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CreatePermitsCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Analytics.Dashboard.Commands;

/// <summary>Defines a new KPI Dashboard.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="DashboardName">Human-readable dashboard name.</param>
/// <param name="DashboardType">Dashboard classification (e.g., Executive, Operator, SiteSafety, Compliance).</param>
/// <param name="OwnerUserId">Identity of the accountable dashboard owner.</param>
/// <param name="RefreshIntervalSeconds">Refresh interval, in seconds, for live data widgets.</param>
/// <param name="WidgetCount">Count of widgets configured on this dashboard.</param>
/// <param name="RequesterUserId">Identity of the user creating this record.</param>
public sealed record CreateDashboardCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string DashboardName,
    string DashboardType,
    Guid OwnerUserId,
    int RefreshIntervalSeconds,
    int WidgetCount,
    Guid RequesterUserId) : ICommand<Result<CreateDashboardResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequesterUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<CreateDashboardResult>>.CausationId => CausationId;

    /// <summary>Builds a <see cref="CreateDashboardCommand"/> from raw, untrusted transport values.</summary>
    public static CreateDashboardCommand FromDto(
        Guid tenantId,
        Guid? siteId,
        string dashboardName,
        string dashboardType,
        Guid ownerUserId,
        int refreshIntervalSeconds,
        int widgetCount,
        Guid requesterUserId,
        Guid? correlationId = null,
        Guid? causationId = null,
        string apiVersionToken = "v1")
        => new(
            CommandId: Guid.NewGuid(),
            CorrelationId: correlationId ?? Guid.NewGuid(),
            CausationId: causationId ?? Guid.NewGuid(),
            RequestedAtUtc: DateTime.UtcNow,
            ApiVersionToken: apiVersionToken,
            TenantId: tenantId,
            SiteId: siteId,
            DashboardName: (dashboardName ?? string.Empty).Trim(),
            DashboardType: (dashboardType ?? string.Empty).Trim(),
            OwnerUserId: ownerUserId,
            RefreshIntervalSeconds: refreshIntervalSeconds,
            WidgetCount: widgetCount,
            RequesterUserId: requesterUserId);
}

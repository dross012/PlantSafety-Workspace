// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Command to remove a Widget placement from a Dashboard's layout.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Dashboards.Dashboard.Commands;

/// <summary>Removes a Widget placement from a Dashboard's layout, per "6. Extensibility: Add/remove widgets".</summary>
/// <param name="CorrelationId">Correlation lineage identifier for this operation group.</param>
/// <param name="CausationId">Identifier of the immediate parent command/event.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued.</param>
/// <param name="DashboardId">Identifier of the dashboard to modify.</param>
/// <param name="WidgetId">Identifier of the widget placement to remove.</param>
/// <param name="RemovedByUserId">Identity of the user modifying the layout.</param>
public sealed record RemoveWidgetFromDashboardCommand(
    Guid CorrelationId,
    Guid? CausationId,
    DateTime RequestedAtUtc,
    Guid DashboardId,
    Guid WidgetId,
    Guid RemovedByUserId) : ICommand<Result<RemoveWidgetFromDashboardResult>>
{
    /// <summary>Gets the unique identifier of this command instance.</summary>
    public Guid CommandId { get; init; } = Guid.NewGuid();
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";
    /// <summary>Placeholder: no tenant field exists on this command; defaults to <see cref="Guid.Empty"/> until tenant scoping is added.</summary>
    public Guid TenantId { get; init; } = Guid.Empty;
    /// <summary>Maps to <see cref="RemovedByUserId"/>.</summary>
    public Guid ActorId => RemovedByUserId;
}

/// <summary>Result of <see cref="RemoveWidgetFromDashboardCommand"/>.</summary>
public sealed record RemoveWidgetFromDashboardResult(Guid DashboardId, Guid WidgetId);

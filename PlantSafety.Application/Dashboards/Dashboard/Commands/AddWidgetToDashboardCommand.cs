// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Command to add a Widget placement to a Dashboard's layout.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Dashboards.Dashboard.Commands;

/// <summary>Adds a Widget placement to a Dashboard's layout, per "6. Extensibility: Add/remove widgets".</summary>
/// <param name="CorrelationId">Correlation lineage identifier for this operation group.</param>
/// <param name="CausationId">Identifier of the immediate parent command/event.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued.</param>
/// <param name="DashboardId">Identifier of the dashboard to modify.</param>
/// <param name="WidgetId">Identifier of the widget to place.</param>
/// <param name="Row">Grid row position (0-based).</param>
/// <param name="Column">Grid column position (0-based).</param>
/// <param name="RowSpan">Grid row span.</param>
/// <param name="ColumnSpan">Grid column span.</param>
/// <param name="FilterOverrides">Optional per-placement filter overrides.</param>
/// <param name="AddedByUserId">Identity of the user modifying the layout.</param>
public sealed record AddWidgetToDashboardCommand(
    Guid CorrelationId,
    Guid? CausationId,
    DateTime RequestedAtUtc,
    Guid DashboardId,
    Guid WidgetId,
    int Row,
    int Column,
    int RowSpan,
    int ColumnSpan,
    IReadOnlyDictionary<string, string>? FilterOverrides,
    Guid AddedByUserId) : ICommand<Result<AddWidgetToDashboardResult>>
{
    /// <summary>Gets the unique identifier of this command instance.</summary>
    public Guid CommandId { get; init; } = Guid.NewGuid();
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";
    /// <summary>Placeholder: no tenant field exists on this command; defaults to <see cref="Guid.Empty"/> until tenant scoping is added.</summary>
    public Guid TenantId { get; init; } = Guid.Empty;
    /// <summary>Maps to <see cref="AddedByUserId"/>.</summary>
    public Guid ActorId => AddedByUserId;
}

/// <summary>Result of <see cref="AddWidgetToDashboardCommand"/>.</summary>
public sealed record AddWidgetToDashboardResult(Guid DashboardId, Guid WidgetId);

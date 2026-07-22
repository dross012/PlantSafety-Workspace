// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Command to create a new (draft) reusable Widget definition.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Dashboards.Widget.Commands;

/// <summary>Creates a new draft Widget definition in the reusable widget library.</summary>
/// <param name="CorrelationId">Correlation lineage identifier for this operation group.</param>
/// <param name="CausationId">Identifier of the immediate parent command/event.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="Name">Human-readable widget name.</param>
/// <param name="TypeCode">Widget visualization type code (e.g. "LineChart", "Gauge").</param>
/// <param name="SourceModule">Owning module the widget's data source belongs to.</param>
/// <param name="QueryOrEndpoint">Query/endpoint identifier within the owning module.</param>
/// <param name="RefreshIntervalSeconds">Optional explicit refresh cadence override, in seconds.</param>
/// <param name="Parameters">Configuration parameters (time range, plant, module, thresholds).</param>
/// <param name="PermittedRoles">Canonical role names permitted to view this widget.</param>
/// <param name="CreatedByUserId">Identity of the user creating this widget.</param>
public sealed record CreateWidgetCommand(
    Guid CorrelationId,
    Guid? CausationId,
    DateTime RequestedAtUtc,
    Guid TenantId,
    string Name,
    string TypeCode,
    string SourceModule,
    string QueryOrEndpoint,
    int? RefreshIntervalSeconds,
    IReadOnlyDictionary<string, string>? Parameters,
    IReadOnlyList<string>? PermittedRoles,
    Guid CreatedByUserId) : ICommand<Result<CreateWidgetResult>>
{
    /// <summary>Gets the unique identifier of this command instance.</summary>
    public Guid CommandId { get; init; } = Guid.NewGuid();
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";
    /// <summary>Maps to <see cref="CreatedByUserId"/>.</summary>
    public Guid ActorId => CreatedByUserId;
}

/// <summary>Result of <see cref="CreateWidgetCommand"/>.</summary>
public sealed record CreateWidgetResult(Guid WidgetId, string Status);

// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Command to deprecate an active Widget.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Dashboards.Widget.Commands;

/// <summary>Deprecates an active Widget definition.</summary>
/// <param name="CorrelationId">Correlation lineage identifier for this operation group.</param>
/// <param name="CausationId">Identifier of the immediate parent command/event.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued.</param>
/// <param name="WidgetId">Identifier of the widget to deprecate.</param>
/// <param name="Reason">Reason for deprecation.</param>
/// <param name="DeprecatedByUserId">Identity of the user deprecating the widget.</param>
public sealed record DeprecateWidgetCommand(
    Guid CorrelationId,
    Guid? CausationId,
    DateTime RequestedAtUtc,
    Guid WidgetId,
    string Reason,
    Guid DeprecatedByUserId) : ICommand<Result<DeprecateWidgetResult>>
{
    /// <summary>Gets the unique identifier of this command instance.</summary>
    public Guid CommandId { get; init; } = Guid.NewGuid();
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";
    /// <summary>Placeholder: no tenant field exists on this command; defaults to <see cref="Guid.Empty"/> until tenant scoping is added.</summary>
    public Guid TenantId { get; init; } = Guid.Empty;
    /// <summary>Maps to <see cref="DeprecatedByUserId"/>.</summary>
    public Guid ActorId => DeprecatedByUserId;
}

/// <summary>Result of <see cref="DeprecateWidgetCommand"/>.</summary>
public sealed record DeprecateWidgetResult(Guid WidgetId, string Status);

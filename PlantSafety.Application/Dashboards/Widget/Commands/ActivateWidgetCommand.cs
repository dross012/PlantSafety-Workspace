// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Command to activate a draft Widget, making it available for placement on dashboards.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Dashboards.Widget.Commands;

/// <summary>Activates a draft Widget.</summary>
/// <param name="CorrelationId">Correlation lineage identifier for this operation group.</param>
/// <param name="CausationId">Identifier of the immediate parent command/event.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued.</param>
/// <param name="WidgetId">Identifier of the widget to activate.</param>
/// <param name="ActivatedByUserId">Identity of the user activating the widget.</param>
public sealed record ActivateWidgetCommand(
    Guid CorrelationId,
    Guid? CausationId,
    DateTime RequestedAtUtc,
    Guid WidgetId,
    Guid ActivatedByUserId) : ICommand<Result<ActivateWidgetResult>>
{
    /// <summary>Gets the unique identifier of this command instance.</summary>
    public Guid CommandId { get; init; } = Guid.NewGuid();
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";
    /// <summary>Placeholder: no tenant field exists on this command; defaults to <see cref="Guid.Empty"/> until tenant scoping is added.</summary>
    public Guid TenantId { get; init; } = Guid.Empty;
    /// <summary>Maps to <see cref="ActivatedByUserId"/>.</summary>
    public Guid ActorId => ActivatedByUserId;
}

/// <summary>Result of <see cref="ActivateWidgetCommand"/>.</summary>
public sealed record ActivateWidgetResult(Guid WidgetId, string Status);

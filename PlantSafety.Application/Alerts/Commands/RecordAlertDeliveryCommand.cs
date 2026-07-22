// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Command to record a multi-channel delivery attempt for an Alert.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Alerts.Commands;

/// <summary>Records a single multi-channel delivery attempt for an Alert, per "12. Multi-Channel Alert Delivery".</summary>
/// <param name="CorrelationId">Correlation lineage identifier for this operation group.</param>
/// <param name="CausationId">Identifier of the immediate parent command/event.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued.</param>
/// <param name="AlertId">Identifier of the alert the delivery attempt belongs to.</param>
/// <param name="ChannelCode">Delivery channel used (e.g. "Sms", "PaSpeaker").</param>
/// <param name="Succeeded">Whether the delivery attempt succeeded.</param>
/// <param name="FailureReason">Sanitized failure reason, when the attempt did not succeed.</param>
/// <param name="RecordedByUserId">Identity of the user/system recording this delivery attempt. Null for automated infrastructure.</param>
public sealed record RecordAlertDeliveryCommand(
    Guid CorrelationId,
    Guid? CausationId,
    DateTime RequestedAtUtc,
    Guid AlertId,
    string ChannelCode,
    bool Succeeded,
    string? FailureReason,
    Guid? RecordedByUserId) : ICommand<Result<RecordAlertDeliveryResult>>
{
    /// <summary>Gets the unique identifier of this command instance.</summary>
    public Guid CommandId { get; init; } = Guid.NewGuid();
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";
    /// <summary>Placeholder: no tenant field exists on this command; defaults to <see cref="Guid.Empty"/> until tenant scoping is added.</summary>
    public Guid TenantId { get; init; } = Guid.Empty;
    /// <summary>Maps to <see cref="RecordedByUserId"/>; falls back to <see cref="Guid.Empty"/> for automated infrastructure recordings.</summary>
    public Guid ActorId => RecordedByUserId ?? Guid.Empty;
}

/// <summary>Result of <see cref="RecordAlertDeliveryCommand"/>.</summary>
public sealed record RecordAlertDeliveryResult(Guid AlertId, string ChannelCode, bool Succeeded);

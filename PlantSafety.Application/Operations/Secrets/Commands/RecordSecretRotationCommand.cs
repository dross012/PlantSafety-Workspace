// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Command recording that a secret rotation was detected/notified (Phase 6.6,
//   §6.3 Enterprise Secrets Management). Rotation itself always happens Azure-side (Key
//   Vault); this command is the platform's record of that fact, carrying only the secret
//   NAME, the rotation timestamp, and the triggering context -- never the secret value.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Operations.Secrets.Commands;

/// <summary>
/// Records that a secret rotation was detected/notified. Never carries the secret value.
/// </summary>
/// <param name="SecretName">The logical name of the secret that was rotated.</param>
/// <param name="RotatedAtUtc">UTC timestamp at which the rotation occurred (or was reported).</param>
/// <param name="TriggeringContext">
/// Human-readable description of what raised this notification (e.g.
/// "EventGrid.WebhookNotification", "Manual.OperatorTriggered").
/// </param>
public sealed record RecordSecretRotationCommand(string SecretName, DateTime RotatedAtUtc, string TriggeringContext)
    : ICommand<Result<bool>>
{
    /// <summary>Gets the unique identifier of this command instance (UUIDv7).</summary>
    public Guid CommandId { get; init; } = Guid.NewGuid();

    /// <summary>Gets the correlation identifier that groups all operations in a single business transaction.</summary>
    public Guid CorrelationId { get; init; } = Guid.NewGuid();

    /// <summary>Gets the causation identifier that identifies the immediate parent command or event.</summary>
    public Guid? CausationId { get; init; }

    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the tenant identifier for multi-tenancy isolation.
    /// Placeholder: secret rotation is server-level platform data, not tenant-scoped; defaults to <see cref="Guid.Empty"/>.</summary>
    public Guid TenantId { get; init; } = Guid.Empty;

    /// <summary>Gets the identifier of the actor who initiated the command.
    /// Placeholder: rotation notifications originate from Key Vault/Event Grid, not a user; defaults to <see cref="Guid.Empty"/>.</summary>
    public Guid ActorId { get; init; } = Guid.Empty;
}

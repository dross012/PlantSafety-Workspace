// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Command to trigger a new Alert.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Alerts.Commands;

/// <summary>
/// Triggers a new Alert of the given catalog category and source type. Severity defaults to the
/// category's canonical default severity unless <paramref name="SeverityCode"/> is supplied.
/// </summary>
/// <param name="CorrelationId">Correlation lineage identifier for this operation group.</param>
/// <param name="CausationId">Identifier of the immediate parent command/event. Null for root operations.</param>
/// <param name="CommandId">Unique identifier for this command instance.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued.</param>
/// <param name="ApiVersionToken">API version token this command was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope.</param>
/// <param name="CategoryCode">Catalog category code (e.g. "LifeSafetyEmergency").</param>
/// <param name="SourceTypeCode">Origin that triggered the alert (Sensor/ComputerVision/Wearable/PanicButton/Human/System).</param>
/// <param name="Title">Short human-readable alert title.</param>
/// <param name="LocationZoneName">Zone/area name where the alert occurred.</param>
/// <param name="Latitude">Optional GPS latitude.</param>
/// <param name="Longitude">Optional GPS longitude.</param>
/// <param name="SeverityCode">Optional explicit severity override.</param>
/// <param name="TriggeredByUserId">CRUD authorship identity. Null for system/sensor-triggered alerts.</param>
public sealed record TriggerAlertCommand(
    Guid CorrelationId,
    Guid? CausationId,
    Guid CommandId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string CategoryCode,
    string SourceTypeCode,
    string Title,
    string LocationZoneName,
    double? Latitude,
    double? Longitude,
    string? SeverityCode,
    Guid? TriggeredByUserId) : ICommand<Result<TriggerAlertResult>>
{
    /// <summary>Maps to <see cref="TriggeredByUserId"/>; falls back to <see cref="Guid.Empty"/> for system/sensor-triggered alerts.</summary>
    public Guid ActorId => TriggeredByUserId ?? Guid.Empty;

    /// <summary>Normalizes a DTO payload into a <see cref="TriggerAlertCommand"/>, trimming strings and validating identifiers.</summary>
    public static TriggerAlertCommand FromDto(
        Guid correlationId, Guid? causationId, Guid tenantId, Guid? siteId,
        string categoryCode, string sourceTypeCode, string title, string locationZoneName,
        double? latitude, double? longitude, string? severityCode, Guid? triggeredByUserId, string apiVersionToken)
        => new(
            correlationId,
            causationId,
            Guid.NewGuid(),
            DateTime.UtcNow,
            apiVersionToken,
            tenantId,
            siteId,
            (categoryCode ?? string.Empty).Trim(),
            (sourceTypeCode ?? string.Empty).Trim(),
            (title ?? string.Empty).Trim(),
            (locationZoneName ?? string.Empty).Trim(),
            latitude,
            longitude,
            string.IsNullOrWhiteSpace(severityCode) ? null : severityCode.Trim(),
            triggeredByUserId);
}

/// <summary>Result of <see cref="TriggerAlertCommand"/>.</summary>
/// <param name="AlertId">Identifier of the newly triggered alert.</param>
/// <param name="Status">Lifecycle status immediately after trigger (always "Triggered").</param>
/// <param name="SeverityCode">Resolved severity code applied to the new alert.</param>
public sealed record TriggerAlertResult(Guid AlertId, string Status, string SeverityCode);

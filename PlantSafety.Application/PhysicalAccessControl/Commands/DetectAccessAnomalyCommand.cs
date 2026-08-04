// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Command to record a newly detected AccessAnomaly ("PhysicalAccessControl.
//   AccessAnomalies"). Invoked internally by anomaly-detection correlation over ingested
//   AccessEvents, and feeds RiskManagement.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PhysicalAccessControl.Commands;

/// <summary>Records a newly detected physical-access anomaly.</summary>
public sealed record DetectAccessAnomalyCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid? SiteId,
    Guid? AccessEventId,
    Guid AccessPointId,
    Guid? CredentialId,
    Guid? SafetyZoneId,
    string AnomalyType,
    decimal SeverityScore,
    Guid PerformedByUserId) : ICommand<Result<AccessAnomalyActionResult>>
{
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    Guid? ICommand<Result<AccessAnomalyActionResult>>.CausationId => CausationId;

    /// <summary>Gets the identifier of the actor who initiated the command (system actor for automated detection).</summary>
    public Guid ActorId => PerformedByUserId;
}

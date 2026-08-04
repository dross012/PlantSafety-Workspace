// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Command to submit-and-validate a SafetyCaseValidation record ("AutonomousVehicleSafety.SafetyCaseValidation").
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.AutonomousVehicleSafety.Commands;

/// <summary>
/// Records a safety-case validation outcome for an autonomous vehicle asset against ANSI/RIA R15.08
/// or ISO 3691-4. Submits (creates) the SafetyCaseValidation record and immediately validates it,
/// computing its expiry from the supplied revalidation cadence -- the single write path exposed for
/// this subdomain, matching the module's real-world "record a completed validation" workflow.
/// </summary>
public sealed record ValidateSafetyCaseCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid? SiteId,
    Guid AutonomousVehicleAssetId,
    string StandardReference,
    string DocumentVersion,
    int RevalidationCadenceDays,
    Guid ValidatedByUserId,
    Guid PerformedByUserId) : ICommand<Result<SafetyCaseValidationActionResult>>
{
    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    Guid? ICommand<Result<SafetyCaseValidationActionResult>>.CausationId => CausationId;

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => PerformedByUserId;
}

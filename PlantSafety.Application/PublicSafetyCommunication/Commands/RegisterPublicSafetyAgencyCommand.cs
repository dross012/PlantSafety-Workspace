// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Command to register a new external public-safety agency (fire, police, EMS,
// regulatory hotline, public alert authority) in the Agency Contact Directory.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PublicSafetyCommunication.Commands;

/// <summary>Registers a new external public-safety agency in the directory.</summary>
/// <param name="AgencyName">Legal/display name of the agency.</param>
/// <param name="AgencyType">Classification of the agency (Fire, Police, EMS, RegulatoryHotline, etc.).</param>
/// <param name="JurisdictionDescription">Description of the jurisdiction/coverage area.</param>
/// <param name="PrimaryPhoneNumber">Primary phone number for general/dispatch contact.</param>
/// <param name="PrimaryEmail">Optional primary email address.</param>
public sealed record RegisterPublicSafetyAgencyCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    Guid TenantId,
    Guid? SiteId,
    string AgencyName,
    string AgencyType,
    string JurisdictionDescription,
    string PrimaryPhoneNumber,
    string? PrimaryEmail) : ICommand<Result<PublicSafetyAgencyActionResult>>
{
    /// <summary>Gets the UTC timestamp when this command was created at the API boundary.</summary>
    public DateTime RequestedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>Gets the API version token to enforce backward-compatible evolution.</summary>
    public string ApiVersionToken { get; init; } = "v1";

    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId { get; init; }

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<PublicSafetyAgencyActionResult>>.CausationId => CausationId;
}

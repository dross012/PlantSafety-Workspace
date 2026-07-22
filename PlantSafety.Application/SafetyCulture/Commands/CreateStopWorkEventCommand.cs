// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Command to create a new StopWorkEvent record within the SafetyCulture module. Carries the full command envelope (CorrelationId, CausationId, CommandId, RequestedAtUtc, ApiVersionToken) per Handbook Rule 2.1.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyCulture.Commands;

/// <summary>
/// Creates a new StopWorkEvent record under the SafetyCulture bounded context.
/// </summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="OwnerId">Identity of the accountable owner / CRUD-authorship user.</param>
/// <param name="WorkArea">Work area/location where the stop-work action was initiated.</param>
/// <param name="HazardDescription">Description of the hazard or unsafe condition that triggered the stop-work action.</param>
public sealed record CreateStopWorkEventCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    Guid OwnerId,
    string WorkArea,
    string HazardDescription) : ICommand<Result<CreateStopWorkEventResult>>
{
    /// <inheritdoc/>
    public Guid ActorId => OwnerId;
    Guid? ICommand<Result<CreateStopWorkEventResult>>.CausationId => CausationId;

    /// <summary>
    /// Builds a <see cref="CreateStopWorkEventCommand"/> from raw, untrusted transport values, normalizing
    /// strings and generating envelope identifiers where the caller did not supply them.
    /// </summary>
    public static CreateStopWorkEventCommand FromDto(
        Guid tenantId,
        Guid? siteId,
        Guid ownerId,
        string workArea,
        string hazardDescription,
        Guid? correlationId = null,
        Guid? causationId = null,
        string apiVersionToken = "v1")
        => new(
            CommandId: Guid.NewGuid(),
            CorrelationId: correlationId ?? Guid.NewGuid(),
            CausationId: causationId ?? Guid.NewGuid(),
            RequestedAtUtc: DateTime.UtcNow,
            ApiVersionToken: apiVersionToken,
            TenantId: tenantId,
            SiteId: siteId,
            OwnerId: ownerId,
            WorkArea: (workArea ?? string.Empty).Trim(),
            HazardDescription: (hazardDescription ?? string.Empty).Trim());
}
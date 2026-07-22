// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// UPDATED: 2026-07-21 -- extended to capture the real report-time fields (location, OSHA
//   severity classification, injured person count, occurrence time) needed by the real
//   Accident aggregate lifecycle, replacing the generic placeholder shape.
// FILE DESCRIPTION: Command to report a new Accident record within the AccidentManagement module.
//   Carries the full command envelope (CorrelationId, CausationId, CommandId, RequestedAtUtc,
//   ApiVersionToken) per Handbook Rule 2.1.
// SchemaVersion: 2

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.AccidentManagement.Commands;

/// <summary>
/// Reports a new Accident under the AccidentManagement bounded context.
/// </summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="AccidentTitle">Human-readable summary of the accident.</param>
/// <param name="Location">Free-form location description (building/zone/machine).</param>
/// <param name="Severity">AccidentSeverity code (see AccidentManagement.Enums.AccidentSeverity).</param>
/// <param name="InjuredPersonCount">Count of individuals injured.</param>
/// <param name="OccurredAtUtc">UTC instant the accident actually occurred.</param>
/// <param name="Category">InvestigationPhase code (see AccidentManagement.Enums.InvestigationPhase) governing this record.</param>
/// <param name="OwnerId">Identity of the accountable owner (reporting supervisor/safety manager).</param>
/// <param name="ReviewRequired">Whether formal review/acknowledgment is required.</param>
public sealed record CreateAccidentCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string AccidentTitle,
    string Location,
    string Severity,
    int InjuredPersonCount,
    DateTime OccurredAtUtc,
    string Category,
    Guid OwnerId,
    bool ReviewRequired) : ICommand<Result<CreateAccidentResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.
    /// Placeholder: no dedicated actor/reported-by field exists on this command yet; defaults to <see cref="Guid.Empty"/>.</summary>
    public Guid ActorId { get; init; } = Guid.Empty;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<CreateAccidentResult>>.CausationId => CausationId;

    /// <summary>
    /// Builds a <see cref="CreateAccidentCommand"/> from raw, untrusted transport values, normalizing
    /// strings and generating envelope identifiers where the caller did not supply them.
    /// </summary>
    public static CreateAccidentCommand FromDto(
        Guid tenantId,
        Guid? siteId,
        string accidentTitle,
        string location,
        string severity,
        int injuredPersonCount,
        DateTime occurredAtUtc,
        string category,
        Guid ownerId,
        bool reviewRequired,
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
            AccidentTitle: (accidentTitle ?? string.Empty).Trim(),
            Location: (location ?? string.Empty).Trim(),
            Severity: (severity ?? string.Empty).Trim(),
            InjuredPersonCount: injuredPersonCount,
            OccurredAtUtc: occurredAtUtc,
            Category: (category ?? string.Empty).Trim(),
            OwnerId: ownerId,
            ReviewRequired: reviewRequired);
}

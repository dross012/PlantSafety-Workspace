// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// UPDATED: 2026-07-21 -- extended to capture the real hazard-identification fields (location,
//   hazard source) needed by Phase 1 Step 1 of the Full Risk Management Process, replacing the
//   generic placeholder shape. Category/RiskLevel is now derived from scoring, not supplied at
//   creation time.
// FILE DESCRIPTION: Command to identify a new hazard/risk within the RiskManagement module.
//   Carries the full command envelope (CorrelationId, CausationId, CommandId, RequestedAtUtc,
//   ApiVersionToken) per Handbook Rule 2.1.
// SchemaVersion: 2

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.RiskManagement.Commands;

/// <summary>
/// Identifies a new hazard/risk under the RiskManagement bounded context (Full Risk Management
/// Process, Phase 1, Step 1).
/// </summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="RiskTitle">Human-readable hazard title/summary.</param>
/// <param name="Location">Free-form location description (building/zone/equipment).</param>
/// <param name="HazardSource">Hazard source classification (mechanical, electrical, chemical, ergonomic, process, human factors).</param>
/// <param name="OwnerId">Identity of the accountable owner.</param>
/// <param name="ReviewRequired">Whether formal review/acknowledgment is required.</param>
public sealed record CreateRiskCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string RiskTitle,
    string Location,
    string HazardSource,
    Guid OwnerId,
    bool ReviewRequired) : ICommand<Result<CreateRiskResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.
    /// Placeholder: no dedicated actor/reported-by field exists on this command yet; defaults to <see cref="Guid.Empty"/>.</summary>
    public Guid ActorId { get; init; } = Guid.Empty;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<CreateRiskResult>>.CausationId => CausationId;

    /// <summary>
    /// Builds a <see cref="CreateRiskCommand"/> from raw, untrusted transport values, normalizing
    /// strings and generating envelope identifiers where the caller did not supply them.
    /// </summary>
    public static CreateRiskCommand FromDto(
        Guid tenantId,
        Guid? siteId,
        string riskTitle,
        string location,
        string hazardSource,
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
            RiskTitle: (riskTitle ?? string.Empty).Trim(),
            Location: (location ?? string.Empty).Trim(),
            HazardSource: (hazardSource ?? string.Empty).Trim(),
            OwnerId: ownerId,
            ReviewRequired: reviewRequired);
}

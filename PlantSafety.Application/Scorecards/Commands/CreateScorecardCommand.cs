// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// UPDATED: 2026-07-21 -- replaced the generic EffectiveAtUtc field with the real reporting-period
//   fields (PeriodType, PeriodStartUtc) needed by the real Scorecard aggregate lifecycle.
// FILE DESCRIPTION: Command to open a new Scorecard reporting period within the Scorecards module.
//   Carries the full command envelope (CorrelationId, CausationId, CommandId, RequestedAtUtc,
//   ApiVersionToken) per Handbook Rule 2.1.
// SchemaVersion: 2

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Scorecards.Commands;

/// <summary>
/// Opens a new Scorecard reporting period under the Scorecards bounded context.
/// </summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="ScorecardTitle">Human-readable title for the new scorecard record.</param>
/// <param name="Category">IndicatorCategory code (see Scorecards.Enums.IndicatorCategory) governing this record's primary panel emphasis.</param>
/// <param name="PeriodType">ScorecardPeriodType code (Monthly/Quarterly/Annual).</param>
/// <param name="PeriodStartUtc">UTC instant the reporting period starts.</param>
/// <param name="OwnerId">Identity of the accountable owner.</param>
/// <param name="ReviewRequired">Whether formal review/acknowledgment is required.</param>
public sealed record CreateScorecardCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string ScorecardTitle,
    string Category,
    string PeriodType,
    DateTime PeriodStartUtc,
    Guid OwnerId,
    bool ReviewRequired) : ICommand<Result<CreateScorecardResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.
    /// Placeholder: no dedicated actor/created-by field exists on this command yet; defaults to <see cref="Guid.Empty"/>.</summary>
    public Guid ActorId { get; init; } = Guid.Empty;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<CreateScorecardResult>>.CausationId => CausationId;

    /// <summary>
    /// Builds a <see cref="CreateScorecardCommand"/> from raw, untrusted transport values, normalizing
    /// strings and generating envelope identifiers where the caller did not supply them.
    /// </summary>
    public static CreateScorecardCommand FromDto(
        Guid tenantId,
        Guid? siteId,
        string scorecardTitle,
        string category,
        string periodType,
        DateTime periodStartUtc,
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
            ScorecardTitle: (scorecardTitle ?? string.Empty).Trim(),
            Category: (category ?? string.Empty).Trim(),
            PeriodType: (periodType ?? string.Empty).Trim(),
            PeriodStartUtc: periodStartUtc,
            OwnerId: ownerId,
            ReviewRequired: reviewRequired);
}

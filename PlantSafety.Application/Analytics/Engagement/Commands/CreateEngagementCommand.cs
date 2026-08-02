// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.Engagement
//  Layer:          Application
//  File:           CreateEngagementCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to submit a new worker Engagement measurement under the
//                  Analytics.Engagement bounded context. Carries the full command envelope
//                  (CorrelationId, CausationId, CommandId, RequestedAtUtc, ApiVersionToken) per
//                  Handbook Rule 2.1. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CreatePermitsCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Analytics.Engagement.Commands;

/// <summary>Submits a new Engagement measurement.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="WorkerId">Worker this engagement measurement relates to.</param>
/// <param name="EngagementScore">Computed engagement score (0-100).</param>
/// <param name="SurveyPeriod">Survey period this measurement covers (e.g., "2026-Q3").</param>
/// <param name="ParticipationChannel">Channel through which engagement was captured (e.g., MobileApp, Kiosk, Paper, Interview).</param>
/// <param name="ResponseCount">Number of responses/interactions captured for this measurement.</param>
/// <param name="LastEngagedAtUtc">UTC timestamp this worker last engaged with a safety touchpoint, if known at creation time.</param>
/// <param name="RequesterUserId">Identity of the worker or supervisor submitting the measurement.</param>
public sealed record CreateEngagementCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    Guid WorkerId,
    decimal EngagementScore,
    string SurveyPeriod,
    string ParticipationChannel,
    int ResponseCount,
    DateTime? LastEngagedAtUtc,
    Guid RequesterUserId) : ICommand<Result<CreateEngagementResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => RequesterUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<CreateEngagementResult>>.CausationId => CausationId;

    /// <summary>Builds a <see cref="CreateEngagementCommand"/> from raw, untrusted transport values.</summary>
    public static CreateEngagementCommand FromDto(
        Guid tenantId,
        Guid? siteId,
        Guid workerId,
        decimal engagementScore,
        string surveyPeriod,
        string participationChannel,
        int responseCount,
        DateTime? lastEngagedAtUtc,
        Guid requesterUserId,
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
            WorkerId: workerId,
            EngagementScore: engagementScore,
            SurveyPeriod: (surveyPeriod ?? string.Empty).Trim(),
            ParticipationChannel: (participationChannel ?? string.Empty).Trim(),
            ResponseCount: responseCount < 0 ? 0 : responseCount,
            LastEngagedAtUtc: lastEngagedAtUtc,
            RequesterUserId: requesterUserId);
}

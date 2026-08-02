// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Analytics.ScoreCards
//  Layer:          Application
//  File:           CreateScoreCardsCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to create a new ScoreCards record under the Analytics.ScoreCards
//                  bounded context. Carries the full command envelope (CorrelationId,
//                  CausationId, CommandId, RequestedAtUtc, ApiVersionToken) per Handbook Rule 2.1.
//                  Mirrors PlantSafety.Application.Compliance.Permits.Commands.CreatePermitsCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Analytics.ScoreCards.Commands;

/// <summary>Submits a new ScoreCards record.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="ScoreCardName">Human-readable scorecard name.</param>
/// <param name="ReportingPeriod">Reporting period this scorecard covers (e.g., "2026-Q3").</param>
/// <param name="KpiCategory">KPI category this scorecard reports on (e.g., Safety, Compliance, Operations).</param>
/// <param name="PerformanceIndex">Computed performance index (0-100).</param>
/// <param name="GeneratedForSiteId">Site this scorecard was generated for, if site-scoped. Null means enterprise-wide.</param>
/// <param name="CreatedByUserId">Identity of the user creating this record.</param>
public sealed record CreateScoreCardsCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string ScoreCardName,
    string ReportingPeriod,
    string KpiCategory,
    decimal PerformanceIndex,
    Guid? GeneratedForSiteId,
    Guid CreatedByUserId) : ICommand<Result<CreateScoreCardsResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => CreatedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<CreateScoreCardsResult>>.CausationId => CausationId;

    /// <summary>Builds a <see cref="CreateScoreCardsCommand"/> from raw, untrusted transport values.</summary>
    public static CreateScoreCardsCommand FromDto(
        Guid tenantId,
        Guid? siteId,
        string scoreCardName,
        string reportingPeriod,
        string kpiCategory,
        decimal performanceIndex,
        Guid? generatedForSiteId,
        Guid createdByUserId,
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
            ScoreCardName: (scoreCardName ?? string.Empty).Trim(),
            ReportingPeriod: (reportingPeriod ?? string.Empty).Trim(),
            KpiCategory: (kpiCategory ?? string.Empty).Trim(),
            PerformanceIndex: performanceIndex,
            GeneratedForSiteId: generatedForSiteId,
            CreatedByUserId: createdByUserId);
}

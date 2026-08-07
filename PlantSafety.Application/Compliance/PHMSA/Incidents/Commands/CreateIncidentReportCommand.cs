// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.PHMSA.Incidents
//  Layer:          Application
//  File:           CreateIncidentReportCommand.cs
//  Created:        2026-08-07
//  SchemaVersion:  2026.1
//  Description:    Command to file a new DOT Form 5800.1 hazardous materials incident report.
//                  Carries the full command envelope (CorrelationId, CausationId, CommandId,
//                  RequestedAtUtc, ApiVersionToken) per Handbook Rule 2.1. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CreatePermitsCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.PHMSA.Incidents.Commands;

/// <summary>Files a new DOT Form 5800.1 hazardous materials incident report.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="IncidentType">Classification of the incident (Spill, Leak, FireOrExplosion, etc.).</param>
/// <param name="MaterialInvolvedId">Optional reference to the classified hazardous material involved.</param>
/// <param name="MaterialInvolvedDescription">Free-text fallback description used when <paramref name="MaterialInvolvedId"/> is null.</param>
/// <param name="Injuries">Count of injuries associated with the incident. Must be &gt;= 0.</param>
/// <param name="FatalitiesInvolved">Whether the incident involved one or more fatalities.</param>
/// <param name="PropertyDamageEstimateUsd">Estimated property damage in USD, if known. Must be &gt;= 0 when provided.</param>
/// <param name="Narrative">Required free-text description of the incident.</param>
/// <param name="IncidentDateUtc">UTC instant the incident occurred.</param>
/// <param name="Location">Physical location the incident occurred at.</param>
/// <param name="ReportedByUserId">Identity of the user filing the report.</param>
public sealed record CreateIncidentReportCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string IncidentType,
    Guid? MaterialInvolvedId,
    string? MaterialInvolvedDescription,
    int Injuries,
    bool FatalitiesInvolved,
    decimal? PropertyDamageEstimateUsd,
    string Narrative,
    DateTime IncidentDateUtc,
    string Location,
    Guid ReportedByUserId) : ICommand<Result<CreateIncidentReportResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => ReportedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<CreateIncidentReportResult>>.CausationId => CausationId;
}

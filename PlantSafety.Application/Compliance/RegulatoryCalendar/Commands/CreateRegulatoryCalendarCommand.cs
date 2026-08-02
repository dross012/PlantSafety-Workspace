// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Compliance.RegulatoryCalendar
//  Layer:          Application
//  File:           CreateRegulatoryCalendarCommand.cs
//  Created:        2026-08-02
//  SchemaVersion:  2026.1
//  Description:    Command to submit a new RegulatoryCalendar obligation record under the
//                  Compliance.RegulatoryCalendar bounded context. Carries the full command
//                  envelope (CorrelationId, CausationId, CommandId, RequestedAtUtc,
//                  ApiVersionToken) per Handbook Rule 2.1. Mirrors
//                  PlantSafety.Application.Compliance.Permits.Commands.CreatePermitsCommand.
//                  Additive-only evolution.
// ============================================================================
using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.RegulatoryCalendar.Commands;

/// <summary>Submits a new RegulatoryCalendar obligation record.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier, constant across the whole request.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event. Null only for root operations.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued, generated once at the API boundary.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="SiteId">Optional site scope. Null applies tenant-wide.</param>
/// <param name="EventTitle">Human-readable title of the regulatory event/obligation.</param>
/// <param name="RegulatoryBody">Governing regulatory body (e.g., "OSHA", "EPA", "State DOL").</param>
/// <param name="JurisdictionCode">Jurisdiction code the obligation applies to (e.g., "US-CA", "US-FED").</param>
/// <param name="ComplianceCategory">Compliance category (e.g., Reporting, Inspection, Renewal, Filing).</param>
/// <param name="ResponsiblePartyId">Identity of the party accountable for meeting this obligation.</param>
/// <param name="DueAtUtc">UTC timestamp the obligation is due, if known.</param>
/// <param name="ReminderLeadDays">Number of days before DueAtUtc that reminders should fire.</param>
/// <param name="CreatedByUserId">Identity of the user submitting the record.</param>
public sealed record CreateRegulatoryCalendarCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TenantId,
    Guid? SiteId,
    string EventTitle,
    string RegulatoryBody,
    string JurisdictionCode,
    string ComplianceCategory,
    Guid ResponsiblePartyId,
    DateTime? DueAtUtc,
    int ReminderLeadDays,
    Guid CreatedByUserId) : ICommand<Result<CreateRegulatoryCalendarResult>>
{
    /// <summary>Gets the identifier of the actor who initiated the command.</summary>
    public Guid ActorId => CreatedByUserId;

    /// <inheritdoc cref="ICommand{TResponse}.CausationId" />
    Guid? ICommand<Result<CreateRegulatoryCalendarResult>>.CausationId => CausationId;

    /// <summary>Builds a <see cref="CreateRegulatoryCalendarCommand"/> from raw, untrusted transport values.</summary>
    public static CreateRegulatoryCalendarCommand FromDto(
        Guid tenantId,
        Guid? siteId,
        string eventTitle,
        string regulatoryBody,
        string jurisdictionCode,
        string complianceCategory,
        Guid responsiblePartyId,
        DateTime? dueAtUtc,
        int reminderLeadDays,
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
            EventTitle: (eventTitle ?? string.Empty).Trim(),
            RegulatoryBody: (regulatoryBody ?? string.Empty).Trim(),
            JurisdictionCode: (jurisdictionCode ?? string.Empty).Trim(),
            ComplianceCategory: (complianceCategory ?? string.Empty).Trim(),
            ResponsiblePartyId: responsiblePartyId,
            DueAtUtc: dueAtUtc,
            ReminderLeadDays: reminderLeadDays < 0 ? 0 : reminderLeadDays,
            CreatedByUserId: createdByUserId);
}

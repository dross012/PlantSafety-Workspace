// ============================================================
// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Client-side DTOs for the PublicSafetyCommunication module, mirroring the
//   PlantSafety.Api read-model shapes for PublicSafetyAgency, IncidentNotification,
//   PublicAlertBroadcast, and CommunicationLogEntry. Display only.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.PublicSafetyCommunication.Models;

/// <summary>A single external public-safety agency directory record.</summary>
public sealed class PublicSafetyAgencyDto
{
    /// <summary>Gets the agency record identifier.</summary>
    public Guid PublicSafetyAgencyId { get; init; }

    /// <summary>Gets the current operational status.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the legal/display name of the agency.</summary>
    public string AgencyName { get; init; } = string.Empty;

    /// <summary>Gets the classification of the agency.</summary>
    public string AgencyType { get; init; } = string.Empty;

    /// <summary>Gets the description of the jurisdiction/coverage area.</summary>
    public string JurisdictionDescription { get; init; } = string.Empty;

    /// <summary>Gets the current verification/currency status of the agency's contact data.</summary>
    public string VerificationStatus { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp of the most recent successful verification, if any.</summary>
    public DateTime? LastVerifiedAtUtc { get; init; }

    /// <summary>Gets the UTC timestamp the next scheduled verification is due.</summary>
    public DateTime? NextVerificationDueAtUtc { get; init; }
}

/// <summary>Response payload for the public-safety-agencies read-model query.</summary>
public sealed class ListPublicSafetyAgenciesResponseDto
{
    /// <summary>Gets the matching agency directory records. Never null; empty when no results.</summary>
    public IReadOnlyList<PublicSafetyAgencyDto> Items { get; init; } = [];
}

/// <summary>A single outbound incident notification dispatch record.</summary>
public sealed class IncidentNotificationDto
{
    /// <summary>Gets the notification record identifier.</summary>
    public Guid IncidentNotificationId { get; init; }

    /// <summary>Gets the current dispatch lifecycle status.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the identity of the receiving public-safety agency.</summary>
    public Guid PublicSafetyAgencyId { get; init; }

    /// <summary>Gets the channel used to deliver this notification.</summary>
    public string ChannelCode { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp the notification was dispatched, if any.</summary>
    public DateTime? DispatchedAtUtc { get; init; }

    /// <summary>Gets the UTC timestamp the agency acknowledged receipt, if any.</summary>
    public DateTime? AcknowledgedAtUtc { get; init; }
}

/// <summary>Response payload for the incident-notification-status read-model query.</summary>
public sealed class GetIncidentNotificationStatusResponseDto
{
    /// <summary>Gets the matching notification records. Never null; empty when no results.</summary>
    public IReadOnlyList<IncidentNotificationDto> Items { get; init; } = [];
}

/// <summary>A single public alert broadcast record.</summary>
public sealed class PublicAlertBroadcastDto
{
    /// <summary>Gets the broadcast record identifier.</summary>
    public Guid PublicAlertBroadcastId { get; init; }

    /// <summary>Gets the current delivery lifecycle status.</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Gets the severity level of this broadcast.</summary>
    public string SeverityCode { get; init; } = string.Empty;

    /// <summary>Gets the target broadcast channel.</summary>
    public string ChannelCode { get; init; } = string.Empty;

    /// <summary>Gets the short headline for the alert.</summary>
    public string Headline { get; init; } = string.Empty;

    /// <summary>Gets the description of the geographic area targeted by this broadcast.</summary>
    public string TargetArea { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp the broadcast was sent, if any.</summary>
    public DateTime? IssuedAtUtc { get; init; }

    /// <summary>Gets the UTC timestamp delivery was confirmed, if any.</summary>
    public DateTime? DeliveryConfirmedAtUtc { get; init; }
}

/// <summary>Response payload for the active-broadcasts read-model query.</summary>
public sealed class ListActiveBroadcastsResponseDto
{
    /// <summary>Gets the matching active broadcast records. Never null; empty when no results.</summary>
    public IReadOnlyList<PublicAlertBroadcastDto> Items { get; init; } = [];
}

/// <summary>A single immutable communication log entry documenting a communication with an external stakeholder.</summary>
public sealed class CommunicationLogEntryDto
{
    /// <summary>Gets the log entry record identifier.</summary>
    public Guid CommunicationLogEntryId { get; init; }

    /// <summary>Gets the direction of the communication.</summary>
    public string Direction { get; init; } = string.Empty;

    /// <summary>Gets the medium used for the communication.</summary>
    public string Medium { get; init; } = string.Empty;

    /// <summary>Gets the name of the external counterparty involved.</summary>
    public string CounterpartyName { get; init; } = string.Empty;

    /// <summary>Gets the free-text summary of the communication content.</summary>
    public string Summary { get; init; } = string.Empty;

    /// <summary>Gets the UTC timestamp the communication actually occurred.</summary>
    public DateTime OccurredAtUtc { get; init; }
}

/// <summary>Response payload for the communication-log-by-incident read-model query.</summary>
public sealed class GetCommunicationLogByIncidentResponseDto
{
    /// <summary>Gets the matching log entries, ordered chronologically. Never null; empty when no results.</summary>
    public IReadOnlyList<CommunicationLogEntryDto> Items { get; init; } = [];
}

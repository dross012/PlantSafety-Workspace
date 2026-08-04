// ============================================================================
// File: PublicSafetyCommunicationRequests.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-04
// Description: Request DTOs bound from the HTTP request body/query string for the
//              PublicSafetyCommunication V1 controller (AgencyDirectory, IncidentNotification
//              Dispatch, PublicAlertBroadcast, CommunicationLog).
// ============================================================================

namespace PlantSafety.Api.PublicSafetyCommunication.Contracts.Requests;

/// <summary>Request body for registering a new external public-safety agency.</summary>
public sealed record RegisterPublicSafetyAgencyRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Legal/display name of the agency.</summary>
    public required string AgencyName { get; init; }

    /// <summary>Classification of the agency (Fire, Police, EmergencyMedicalServices, RegulatoryHotline, PublicAlertAuthority, HazmatResponse, PoisonControl, EmergencyManagementAgency).</summary>
    public required string AgencyType { get; init; }

    /// <summary>Description of the jurisdiction/coverage area.</summary>
    public required string JurisdictionDescription { get; init; }

    /// <summary>Primary phone number for general/dispatch contact.</summary>
    public required string PrimaryPhoneNumber { get; init; }

    /// <summary>Optional primary email address.</summary>
    public string? PrimaryEmail { get; init; }
}

/// <summary>Request body for dispatching an outbound incident notification to an external agency.</summary>
public sealed record DispatchIncidentNotificationRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Optional identity of the originating incident.</summary>
    public Guid? IncidentId { get; init; }

    /// <summary>Identity of the receiving PublicSafetyAgency.</summary>
    public required Guid PublicSafetyAgencyId { get; init; }

    /// <summary>Stable code of the PublicSafetyDeliveryChannel to use (Phone, Fax, Email, Sms, RadioDispatch, CadIntegration, InPersonLiaison).</summary>
    public required string DeliveryChannelCode { get; init; }

    /// <summary>Notification message content.</summary>
    public required string Message { get; init; }
}

/// <summary>Request body for recording an agency's acknowledgement of a dispatched notification.</summary>
public sealed record AcknowledgeIncidentNotificationRequest
{
    /// <summary>Name/desk of the agency contact who acknowledged receipt.</summary>
    public required string AcknowledgedByContactName { get; init; }

    /// <summary>Optimistic-concurrency token from the last read of this IncidentNotification. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for closing an incident notification's dispatch lifecycle.</summary>
public sealed record CloseIncidentNotificationRequest
{
    /// <summary>Optimistic-concurrency token from the last read of this IncidentNotification. Null skips the check.</summary>
    public string? ETag { get; init; }
}

/// <summary>Request body for sending a public alert broadcast to a public warning system.</summary>
public sealed record BroadcastPublicAlertRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Optional identity of the originating incident.</summary>
    public Guid? IncidentId { get; init; }

    /// <summary>Stable code of the PublicAlertSeverity (Advisory, Watch, Warning, Emergency).</summary>
    public required string SeverityCode { get; init; }

    /// <summary>Stable code of the PublicBroadcastChannel (EmergencyAlertSystem, WirelessEmergencyAlerts, Ipaws, SirenNetwork, SocialMedia).</summary>
    public required string ChannelCode { get; init; }

    /// <summary>Short headline for the alert.</summary>
    public required string Headline { get; init; }

    /// <summary>Full alert message content.</summary>
    public required string Message { get; init; }

    /// <summary>Description of the geographic area targeted by this broadcast.</summary>
    public required string TargetArea { get; init; }
}

/// <summary>Request body for logging an immutable communication entry with an external stakeholder.</summary>
public sealed record LogCommunicationEntryRequest
{
    /// <summary>Optional site scope. Null applies tenant-wide.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Optional identity of the originating incident.</summary>
    public Guid? IncidentId { get; init; }

    /// <summary>Optional identity of the related IncidentNotification.</summary>
    public Guid? RelatedIncidentNotificationId { get; init; }

    /// <summary>Optional identity of the related PublicAlertBroadcast.</summary>
    public Guid? RelatedPublicAlertBroadcastId { get; init; }

    /// <summary>Direction of the communication (Outbound, Inbound).</summary>
    public required string Direction { get; init; }

    /// <summary>Medium used for the communication (Phone, Email, Fax, Sms, RadioDispatch, CadIntegration, InPerson, PublicAlertSystem).</summary>
    public required string Medium { get; init; }

    /// <summary>Name of the external counterparty (agency/contact) involved.</summary>
    public required string CounterpartyName { get; init; }

    /// <summary>Free-text summary of the communication content.</summary>
    public required string Summary { get; init; }

    /// <summary>UTC timestamp the communication actually occurred.</summary>
    public required DateTime OccurredAtUtc { get; init; }
}

/// <summary>Query-string bound filters for the PublicSafetyAgency directory list endpoint.</summary>
public sealed record ListPublicSafetyAgenciesRequest
{
    /// <summary>Optional site filter. Null returns data for all accessible sites.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Optional agency-type filter. Null returns all types.</summary>
    public string? AgencyType { get; init; }

    /// <summary>When <see langword="true"/>, returns only Active agencies. Defaults to <see langword="false"/>.</summary>
    public bool ActiveOnly { get; init; }
}

/// <summary>Query-string bound filters for the IncidentNotification status list endpoint.</summary>
public sealed record IncidentNotificationStatusRequest
{
    /// <summary>Optional site filter. Null returns data for all accessible sites.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Optional incident filter. Null returns data for all incidents.</summary>
    public Guid? IncidentId { get; init; }

    /// <summary>Inclusive start of the date range (UTC). Defaults to 90 days before now.</summary>
    public DateTime? From { get; init; }

    /// <summary>Inclusive end of the date range (UTC). Defaults to 90 days after now.</summary>
    public DateTime? To { get; init; }
}

/// <summary>Query-string bound filters for the active broadcasts list endpoint.</summary>
public sealed record ListActiveBroadcastsRequest
{
    /// <summary>Optional site filter. Null returns data for all accessible sites.</summary>
    public Guid? SiteId { get; init; }
}

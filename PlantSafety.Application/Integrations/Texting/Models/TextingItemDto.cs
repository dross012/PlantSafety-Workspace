// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Read-model item DTO representing an SMS/text message delivery record sent via an external SMS gateway.
// SchemaVersion: 1

namespace PlantSafety.Application.Integrations.Texting.Models;

/// <summary>Represents an SMS/text message delivery record sent via an external SMS gateway.</summary>
public sealed record TextingItemDto
{
    /// <summary>Identifier of the text message delivery.</summary>
    public required Guid MessageId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Destination phone number in E.164 format.</summary>
    public required string RecipientPhoneNumber { get; init; }

    /// <summary>Template used to compose the message (e.g., IncidentAlert, ShiftReminder).</summary>
    public required string MessageTemplate { get; init; }

    /// <summary>External SMS gateway provider used to send the message (e.g., Twilio).</summary>
    public required string GatewayProvider { get; init; }

    /// <summary>Current delivery status (e.g., Queued/Sent/Delivered/Failed).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the message was sent.</summary>
    public required DateTime SentAtUtc { get; init; }

    /// <summary>Identity of the user or service principal that initiated the text.</summary>
    public required Guid InitiatedBy { get; init; }
}

// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Read-model item DTO for a single outbound/inbound email integration activity record.
// SchemaVersion: 1

namespace PlantSafety.Application.Integrations.Email.Models;

/// <summary>A single email delivery record sent or received via an external email gateway integration.</summary>
public sealed record EmailItemDto
{
    /// <summary>Email delivery record identifier.</summary>
    public required Guid EmailId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Recipient email address.</summary>
    public required string Recipient { get; init; }

    /// <summary>Subject line of the email.</summary>
    public required string Subject { get; init; }

    /// <summary>External email gateway/provider used for delivery (e.g., SendGrid, SMTP relay).</summary>
    public required string Provider { get; init; }

    /// <summary>Current delivery status of the email (e.g., Delivered/Bounced/Pending).</summary>
    public required string Status { get; init; }

    /// <summary>UTC timestamp when the email was sent.</summary>
    public required DateTime SentAtUtc { get; init; }

    /// <summary>Identity of the user or system that triggered the email send.</summary>
    public required Guid SentBy { get; init; }
}

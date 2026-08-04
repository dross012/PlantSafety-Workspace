// ============================================================
// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Client-side DTOs for the CommunitySafety module, mirroring the
//   PlantSafety.Api read-model shapes for CommunityConcern, CommunityNotification, and
//   CommunityImpactAssessment. Display only.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;
using System.Collections.Generic;

namespace PlantSafety.UI.Modules.CommunitySafety.Models;

/// <summary>A single neighbor complaint/concern intake record.</summary>
public sealed class CommunityConcernDto
{
    /// <summary>Gets the concern record identifier.</summary>
    public Guid CommunityConcernId { get; init; }

    /// <summary>Gets the concern classification.</summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>Gets the free-text severity.</summary>
    public string Severity { get; init; } = string.Empty;

    /// <summary>Gets the facility area the concern relates to.</summary>
    public string FacilityArea { get; init; } = string.Empty;

    /// <summary>Gets the narrative description of the concern.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Gets the weak reference to the related SafetyEvents incident, if any.</summary>
    public Guid? RelatedIncidentId { get; init; }

    /// <summary>Gets the overall lifecycle status.</summary>
    public string ConcernStatus { get; init; } = string.Empty;

    /// <summary>Gets the UTC instant the concern was logged.</summary>
    public DateTime LoggedAtUtc { get; init; }
}

/// <summary>Response payload for the open-community-concerns read-model query.</summary>
public sealed class CommunityConcernsResponseDto
{
    /// <summary>Gets the matching community concern records. Never null; empty when no results.</summary>
    public IReadOnlyList<CommunityConcernDto> Items { get; init; } = [];
}

/// <summary>A single community outreach/notification history record.</summary>
public sealed class CommunityNotificationDto
{
    /// <summary>Gets the notification record identifier.</summary>
    public Guid CommunityNotificationId { get; init; }

    /// <summary>Gets the delivery channel used.</summary>
    public string Channel { get; init; } = string.Empty;

    /// <summary>Gets the notification subject line.</summary>
    public string Subject { get; init; } = string.Empty;

    /// <summary>Gets the overall lifecycle status.</summary>
    public string NotificationStatus { get; init; } = string.Empty;

    /// <summary>Gets the UTC instant the notification was sent.</summary>
    public DateTime SentAtUtc { get; init; }

    /// <summary>Gets the UTC instant the recipient acknowledged the notification, if acknowledged.</summary>
    public DateTime? AcknowledgedAtUtc { get; init; }
}

/// <summary>Response payload for the community-notifications-by-period read-model query.</summary>
public sealed class CommunityNotificationsResponseDto
{
    /// <summary>Gets the matching notification records. Never null; empty when no results.</summary>
    public IReadOnlyList<CommunityNotificationDto> Items { get; init; } = [];
}

/// <summary>A single community impact assessment record for an on-site incident.</summary>
public sealed class CommunityImpactAssessmentDto
{
    /// <summary>Gets the assessment record identifier.</summary>
    public Guid CommunityImpactAssessmentId { get; init; }

    /// <summary>Gets the weak reference to the SafetyEvents incident this assessment relates to.</summary>
    public Guid IncidentId { get; init; }

    /// <summary>Gets the assessed severity of the community impact.</summary>
    public string Severity { get; init; } = string.Empty;

    /// <summary>Gets the geographic/community area affected.</summary>
    public string AffectedArea { get; init; } = string.Empty;

    /// <summary>Gets the estimated number of community members affected, if known.</summary>
    public int? EstimatedPersonsAffected { get; init; }

    /// <summary>Gets the narrative description of the assessed impact.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Gets whether this assessment triggers EPCRA/community right-to-know reporting obligations.</summary>
    public bool RequiresRightToKnowReporting { get; init; }

    /// <summary>Gets the overall lifecycle status.</summary>
    public string AssessmentStatus { get; init; } = string.Empty;

    /// <summary>Gets the UTC instant the assessment was recorded.</summary>
    public DateTime AssessedAtUtc { get; init; }
}

/// <summary>Response payload for the impact-assessments-by-incident read-model query.</summary>
public sealed class CommunityImpactAssessmentsResponseDto
{
    /// <summary>Gets the matching impact assessment records. Never null; empty when no results.</summary>
    public IReadOnlyList<CommunityImpactAssessmentDto> Items { get; init; } = [];
}

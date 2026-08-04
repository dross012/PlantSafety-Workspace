// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Request DTOs for the CommunitySafety V1 controller. Each record carries only
//   the domain-specific fields for one command/query; envelope fields (CommandId, CorrelationId,
//   TenantId, ActorId, etc.) are populated server-side via RequestEnvelopeFactory, and aggregate
//   identifiers are bound from the route, not the body.
// SchemaVersion: 1

namespace PlantSafety.Api.CommunitySafety.Contracts.Requests;

/// <summary>Shared query-string filter for period-scoped CommunitySafety "Get"/"List" queries.</summary>
/// <param name="SiteId">Optional site filter. Null returns data for all accessible sites.</param>
/// <param name="From">Inclusive start of the date range (UTC).</param>
/// <param name="To">Inclusive end of the date range (UTC).</param>
public sealed record CommunitySafetyPeriodQueryRequest(Guid? SiteId, DateTime From, DateTime To);

// ── CommunityContact ─────────────────────────────────────────────────────────
public sealed record RegisterCommunityContactRequest(
    Guid? SiteId,
    string DisplayName,
    string ContactType,
    string CommunityArea,
    string Email,
    string? Organization,
    string? Phone,
    bool OptedInForNotifications);

// ── CommunityConcern ─────────────────────────────────────────────────────────
public sealed record LogCommunityConcernRequest(
    Guid SiteId,
    string Category,
    string Severity,
    string FacilityArea,
    string Description,
    Guid? ReportedByContactId,
    Guid? RelatedIncidentId);

public sealed record ResolveCommunityConcernRequest(string ResolutionNotes);

// ── CommunityImpactAssessment ────────────────────────────────────────────────
public sealed record RecordCommunityImpactAssessmentRequest(
    Guid SiteId,
    Guid IncidentId,
    string Severity,
    string AffectedArea,
    string Description,
    int? EstimatedPersonsAffected,
    Guid AssessedByUserId);

// ── CommunityNotification ────────────────────────────────────────────────────
public sealed record SendCommunityNotificationRequest(
    Guid? SiteId,
    string Channel,
    Guid RecipientContactId,
    string Subject,
    string MessageBody,
    Guid? RelatedConcernId,
    Guid? RelatedIncidentId);

// ── CommunityMeetingRecord ───────────────────────────────────────────────────
public sealed record RecordCommunityMeetingRequest(
    Guid SiteId,
    string MeetingTitle,
    DateTime MeetingDateUtc,
    string Location,
    int AttendeeCount,
    string TopicsSummary,
    string? ActionItems,
    Guid RecordedByUserId);

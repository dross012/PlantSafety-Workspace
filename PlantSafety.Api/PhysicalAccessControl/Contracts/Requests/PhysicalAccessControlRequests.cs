// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Request DTOs for the PhysicalAccessControl V1 controller. Each record
//   carries only the domain-specific fields for one command/query; envelope fields (CommandId,
//   CorrelationId, TenantId, ActorId, etc.) are populated server-side via RequestEnvelopeFactory,
//   and aggregate identifiers are bound from the route, not the body.

namespace PlantSafety.Api.PhysicalAccessControl.Contracts.Requests;

// ── AccessPoints ──────────────────────────────────────────────────────────────
public sealed record RegisterAccessPointRequest(
    Guid? SiteId, string AccessPointCode, string Description, string DeviceType, Guid? SafetyZoneId, bool IsPerimeter);

// ── Credentials ───────────────────────────────────────────────────────────────
public sealed record IssueCredentialRequest(
    Guid? SiteId, string BadgeNumber, Guid HolderUserId, string HolderType, string AccessLevel, DateTime? ExpiresAtUtc);
public sealed record RevokeCredentialRequest(string Reason);

// ── AccessEvents ──────────────────────────────────────────────────────────────
public sealed record IngestAccessEventRequest(
    Guid? SiteId, Guid AccessPointId, Guid? CredentialId, Guid? SafetyZoneId, string EventType, string Direction, DateTime OccurredAtUtc);
public sealed record FlagAccessEventAnomalyRequest(string AnomalyType);
public sealed record ListAccessEventsByZoneRequest(Guid SafetyZoneId, DateTime From, DateTime To);

// ── SafetyZones ───────────────────────────────────────────────────────────────
public sealed record RegisterSafetyZoneRequest(
    Guid? SiteId, string ZoneCode, string ZoneName, string HazardLevel, int MaxOccupancy);

// ── AccessAnomalies ───────────────────────────────────────────────────────────
public sealed record DetectAccessAnomalyRequest(
    Guid? SiteId, Guid? AccessEventId, Guid AccessPointId, Guid? CredentialId, Guid? SafetyZoneId, string AnomalyType, decimal SeverityScore);
public sealed record AcknowledgeAccessAnomalyRequest(Guid AcknowledgedByUserId);
public sealed record ListActiveAnomaliesRequest(Guid? SiteId);

// ── RestrictedZonePolicies ────────────────────────────────────────────────────
public sealed record DefineRestrictedZonePolicyRequest(
    Guid? SiteId, Guid SafetyZoneId, string PolicyName, string AllowedAccessLevel, bool EnforceAfterHoursLockout);

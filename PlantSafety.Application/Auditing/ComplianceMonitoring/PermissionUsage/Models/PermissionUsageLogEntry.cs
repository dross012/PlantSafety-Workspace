// AUTHOR: PlantSafety Platform Team (Claude Code performance-optimization-agent)
// CREATED: 2026-08-03
// FILE DESCRIPTION: A single authorization decision record (Handbook Phase 6 §5.4 Compliance
//                    Monitoring -- "access logs"/"permission usage"). Written for every
//                    IAuthorizedRequest evaluated by AuthorizationBehavior, allow or deny.
// SchemaVersion: 1

namespace PlantSafety.Application.Auditing.ComplianceMonitoring.PermissionUsage.Models;

/// <summary>A single authorization (permission-check) decision, allow or deny.</summary>
/// <param name="TenantId">Tenant the request was scoped to.</param>
/// <param name="UserId">The authenticated user the decision was evaluated for.</param>
/// <param name="Permission">The fully-qualified permission name that was checked.</param>
/// <param name="RequestType">The MediatR request type name that triggered the check.</param>
/// <param name="Decision">"Allowed" or "Denied".</param>
/// <param name="CorrelationId">The request's correlation identifier.</param>
/// <param name="OccurredAtUtc">UTC instant the decision was made.</param>
public sealed record PermissionUsageLogEntry(
    Guid TenantId,
    Guid UserId,
    string Permission,
    string RequestType,
    string Decision,
    Guid CorrelationId,
    DateTime OccurredAtUtc);

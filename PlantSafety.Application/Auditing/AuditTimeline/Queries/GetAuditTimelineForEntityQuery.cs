// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-02
// FILE DESCRIPTION: Query to reconstruct the full, chronologically-ordered AuditTimeline for a
//   single entity/aggregate (Handbook Phase 5 §6.1): event timeline, state-change history,
//   concurrency-conflict history, and scheduling-decision history, merged into one coherent view.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Auditing.AuditTimeline.Models;

namespace PlantSafety.Application.Auditing.AuditTimeline.Queries;

/// <summary>Reconstructs the full chronological AuditTimeline for a single entity/aggregate.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="EntityType">Type discriminator of the entity (e.g. "Risk", "Permit", "AuditFinding").</param>
/// <param name="EntityId">Identity of the entity.</param>
public sealed record GetAuditTimelineForEntityQuery(
    Guid TenantId,
    string EntityType,
    Guid EntityId) : IRequest<Result<AuditTimelineDto>>;

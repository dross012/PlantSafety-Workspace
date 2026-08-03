// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Public read-model request envelope for AuditTimeline's
//   IReportSectionProvider&lt;AuditTimelineSectionRequest, AuditTimelineDto&gt; contract (Handbook
//   Part IX §9.3.2 -- read models are an explicitly allowed cross-module integration mechanism).
//   Carries only the scoping parameters required to reconstruct one entity's timeline; contains no
//   domain logic.
// SchemaVersion: 1

namespace PlantSafety.Application.Auditing.AuditTimeline.Contracts;

/// <summary>
/// Deterministic request envelope scoping an AuditTimeline read-model section to a single
/// tenant/entity, for composition into another submodule's aggregate report (e.g. AuditReport).
/// </summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="EntityType">Type discriminator of the entity (e.g. "Risk", "Permit", "AuditFinding").</param>
/// <param name="EntityId">Identity of the entity.</param>
public sealed record AuditTimelineSectionRequest(Guid TenantId, string EntityType, Guid EntityId);

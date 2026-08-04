// AUTHOR: PlantSafety Platform Team (Security & Compliance Agent, Phase 9 §5.6 Compliance Report Generation)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Deterministic request envelope scoping the PermissionUsage tenant audit-stream
//   section for cross-module composition (Handbook Part IX §9.3.2). Owned by this submodule;
//   contains no domain logic.
// SchemaVersion: 1

namespace PlantSafety.Application.Auditing.ComplianceMonitoring.PermissionUsage.Contracts;

/// <summary>Scopes the PermissionUsage tenant audit-stream section for cross-module composition.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="FromUtc">Inclusive UTC range start.</param>
/// <param name="ToUtc">Inclusive UTC range end.</param>
public sealed record TenantAuditStreamSectionRequest(Guid TenantId, DateTime FromUtc, DateTime ToUtc);

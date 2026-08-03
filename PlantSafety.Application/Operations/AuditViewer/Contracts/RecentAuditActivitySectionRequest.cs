// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Public read-model request envelope for Operations.AuditViewer's
//   IReportSectionProvider<RecentAuditActivitySectionRequest, int> contract (Handbook Part IX
//   §9.3.2 -- read models are an explicitly allowed cross-module integration mechanism). Carries
//   only the scoping parameter required by composing modules; contains no domain logic.
// SchemaVersion: 1

namespace PlantSafety.Application.Operations.AuditViewer.Contracts;

/// <summary>
/// Deterministic request envelope scoping a "recent AuditLog activity count" read-model section,
/// for composition into another submodule's aggregate report (e.g. Operations.SystemHealth).
/// </summary>
/// <param name="SinceUtc">Inclusive lower bound on ChangeTime (UTC) for the count.</param>
public sealed record RecentAuditActivitySectionRequest(DateTime SinceUtc);

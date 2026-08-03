// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Public read-model request envelope for Operations.Archival's
//   IReportSectionProvider<PurgeStatusSectionRequest, IReadOnlyList<ArchivalJobDto>> contract
//   (Handbook Part IX §9.3.2 -- read models are an explicitly allowed cross-module integration
//   mechanism). Carries only the scoping parameters required by composing modules; contains no
//   domain logic.
// SchemaVersion: 1

namespace PlantSafety.Application.Operations.Archival.Contracts;

/// <summary>
/// Deterministic request envelope scoping a purge-run-history read-model section, for composition
/// into another submodule's aggregate report (e.g. Operations.SystemHealth).
/// </summary>
/// <param name="Zone">Optional exact Archival.TierPolicy.Zone filter. Null returns all zones.</param>
/// <param name="Top">Maximum number of rows to return. Defaults to 50.</param>
public sealed record PurgeStatusSectionRequest(string? Zone, int Top = 50);

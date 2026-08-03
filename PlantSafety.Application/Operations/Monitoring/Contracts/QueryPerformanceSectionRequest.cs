// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Public read-model request envelope for Operations.Monitoring's
//   IReportSectionProvider<QueryPerformanceSectionRequest, IReadOnlyList<QueryPerformanceItemDto>>
//   contract (Handbook Part IX §9.3.2 -- read models are an explicitly allowed cross-module
//   integration mechanism). Carries only the scoping parameters required by composing modules;
//   contains no domain logic.
// SchemaVersion: 1

namespace PlantSafety.Application.Operations.Monitoring.Contracts;

/// <summary>
/// Deterministic request envelope scoping a Query Store performance read-model section, for
/// composition into another submodule's aggregate report (e.g. Operations.SystemHealth).
/// </summary>
/// <param name="DatabaseKey">Logical database key (see MonitoringOptions.DatabaseConnectionStrings), e.g. "CoreDB".</param>
/// <param name="Top">Maximum number of rows to return. Defaults to 20.</param>
/// <param name="LookbackHours">How far back to look, in hours. Defaults to 24.</param>
public sealed record QueryPerformanceSectionRequest(string DatabaseKey, int Top = 20, int LookbackHours = 24);

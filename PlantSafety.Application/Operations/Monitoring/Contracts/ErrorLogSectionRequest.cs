// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Public read-model request envelope for Operations.Monitoring's
//   IReportSectionProvider<ErrorLogSectionRequest, IReadOnlyList<ErrorLogItemDto>> contract
//   (Handbook Part IX §9.3.2 -- read models are an explicitly allowed cross-module integration
//   mechanism). Carries only the scoping parameters required by composing modules; contains no
//   domain logic.
// SchemaVersion: 1

namespace PlantSafety.Application.Operations.Monitoring.Contracts;

/// <summary>
/// Deterministic request envelope scoping an ErrorLog read-model section, for composition into
/// another submodule's aggregate report (e.g. Operations.SystemHealth).
/// </summary>
/// <param name="MinSeverity">Optional minimum ErrorSeverity (inclusive). Null returns all severities.</param>
/// <param name="SinceUtc">Optional lower bound on ErrorDate (inclusive). Null returns all history.</param>
/// <param name="Top">Page size. Defaults to 50.</param>
public sealed record ErrorLogSectionRequest(int? MinSeverity, DateTime? SinceUtc, int Top = 50);

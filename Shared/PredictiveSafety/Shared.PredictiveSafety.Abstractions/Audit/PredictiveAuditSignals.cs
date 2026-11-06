// ============================================================================
//  Project:        Shared.PredictiveSafety.Abstractions
//  Module:         Audit
//  File:           PredictiveAuditSignals.cs
//  Created:        2026-08-10
//  Description:    Phase 15 deliverable. The deterministic inputs
//                  PredictiveAuditInferenceEngine reacts to. No standalone
//                  Audit domain exists in this program -- confirmed before
//                  writing this file (no IAuditLogRepository/AuditTelemetry
//                  implementation exists anywhere, only a stray metric name
//                  reference; see this module's README for the full record).
//                  These signals are therefore deterministic counts over the
//                  one real, existing structured-record shape every module
//                  already produces: Shared.Telemetry's LogEvent (Phase 2.4)
//                  -- the closest honest substitute for "audit search
//                  metadata", the same kind of documented substitution
//                  Phase 13 made for "Diagnostics".
// ============================================================================
namespace Shared.PredictiveSafety.Abstractions.Audit;

/// <summary>The deterministic LogEvent-derived counts a predictive audit evaluation reacts to.</summary>
public sealed record PredictiveAuditSignals(
    string CorrelationId,
    int TotalLogEventCount,
    int WarningOrAboveLogEventCount,
    int DistinctEventIdCount,
    int GovernanceViolationCount);

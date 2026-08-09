// ============================================================================
//  Project:        Shared.Telemetry
//  Module:         Abstractions.Correlation
//  File:           ICorrelationContext.cs
//  Created:        2026-08-09
//  Description:    Phase 2.4 deliverable: telemetry correlation. Registered
//                  Scoped (one per request in PlantSafety.UI.Api), set once
//                  by CorrelationIdMiddleware at the very start of the
//                  pipeline, then read by anything downstream in that same
//                  request -- controllers, GlobalErrorHandler, engine log
//                  events -- without needing the correlation id threaded
//                  through every method signature by hand. This is the
//                  seam the spec's "API -> WorkflowEngine -> SQL -> AI
//                  Engine -> Enterprise Engine" correlation chain runs
//                  through: every one of those layers, when it logs
//                  through ILogEventWriter, reads the same CorrelationId
//                  from here.
// ============================================================================
namespace Shared.Telemetry.Abstractions.Correlation;

/// <summary>The current request's correlation id, ambient for the lifetime of one request.</summary>
public interface ICorrelationContext
{
    /// <summary>The current request's correlation id.</summary>
    string CorrelationId { get; }
}

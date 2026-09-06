// ============================================================================
//  Project:        Shared.Authorization
//  Module:         Abstractions.Caller
//  File:           ICallerContextProvider.cs
//  Created:        2026-08-10
//  Description:    Phase 10 deliverable. Mirrors Shared.Telemetry.Abstractions
//                  .Correlation.ICorrelationContext's "Set-once-then-read"
//                  pattern exactly, applied to CallerContext: the
//                  authentication middleware calls Set(...) once, near the
//                  start of the pipeline, after which every downstream
//                  consumer (controllers, domain authorization wrappers)
//                  reads Current instead of constructing a CallerContext
//                  inline. Not a second identity mechanism -- just the
//                  scoped carrier for the one CallerContext already defined
//                  in Phase 9.
// ============================================================================
namespace Shared.Authorization.Abstractions.Caller;

/// <summary>The request-scoped carrier for the current request's resolved CallerContext.</summary>
public interface ICallerContextProvider
{
    /// <summary>The current request's resolved CallerContext. Throws if read before Set(...) is called.</summary>
    CallerContext Current { get; }
}

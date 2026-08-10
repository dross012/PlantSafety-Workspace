// ============================================================================
//  Project:        Shared.Authorization.Abstractions
//  Module:         Caller
//  File:           ICallerContextProvider.cs
//  Created:        2026-08-10
//  Description:    Phase 10 deliverable. The read-side seam controllers use
//                  (ICallerContextProvider) so they depend on an interface,
//                  not the concrete CallerContextProvider ApiKeyAuthenticationMiddleware
//                  writes to -- exactly the ICorrelationContext/CorrelationContext
//                  split this program already established for correlation
//                  propagation.
// ============================================================================
namespace Shared.Authorization.Abstractions.Caller;

/// <summary>The read-side seam for the current request's resolved CallerContext.</summary>
public interface ICallerContextProvider
{
    /// <summary>The current request's resolved CallerContext. Throws if read before ApiKeyAuthenticationMiddleware has set it.</summary>
    CallerContext Current { get; }
}

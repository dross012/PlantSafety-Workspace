// ============================================================================
//  Project:        Shared.Authorization
//  Module:         Abstractions.Caller
//  File:           CallerContextProvider.cs
//  Created:        2026-08-10
//  Description:    Phase 10 deliverable. Concrete, scoped implementation of
//                  ICallerContextProvider -- structurally identical to
//                  Shared.Telemetry.Abstractions.Correlation.CorrelationContext
//                  (private nullable backing field, Set(...) called once by
//                  middleware, Current throws InvalidOperationException if
//                  read first).
// ============================================================================
namespace Shared.Authorization.Abstractions.Caller;

/// <summary>Scoped, Set-once-then-read carrier for the current request's resolved CallerContext.</summary>
public sealed class CallerContextProvider : ICallerContextProvider
{
    private CallerContext? _current;

    /// <inheritdoc />
    public CallerContext Current => _current ?? throw new InvalidOperationException(
        "CallerContextProvider.Set(...) must be called (by ApiKeyAuthenticationMiddleware) before Current is read.");

    /// <summary>Sets the current request's resolved CallerContext. Called exactly once, by ApiKeyAuthenticationMiddleware.</summary>
    public void Set(CallerContext callerContext) => _current = callerContext;
}

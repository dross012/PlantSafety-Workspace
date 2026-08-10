// ============================================================================
//  Project:        Shared.Authorization.Abstractions
//  Module:         Caller
//  File:           CallerContextProvider.cs
//  Created:        2026-08-10
//  Description:    Phase 10 deliverable. Request-scoped write-once holder --
//                  registered AddScoped in Program.cs, Set once by
//                  ApiKeyAuthenticationMiddleware, read by controllers via
//                  ICallerContextProvider.Current.
// ============================================================================
namespace Shared.Authorization.Abstractions.Caller;

/// <summary>Request-scoped write-once holder for the resolved CallerContext.</summary>
public sealed class CallerContextProvider : ICallerContextProvider
{
    private CallerContext? _current;

    /// <inheritdoc />
    public CallerContext Current => _current
        ?? throw new InvalidOperationException("CallerContext has not been set for this request. ApiKeyAuthenticationMiddleware must run before it is read.");

    /// <summary>Sets the resolved CallerContext for this request. Called once, by ApiKeyAuthenticationMiddleware.</summary>
    public void Set(CallerContext caller)
    {
        ArgumentNullException.ThrowIfNull(caller);
        _current = caller;
    }
}

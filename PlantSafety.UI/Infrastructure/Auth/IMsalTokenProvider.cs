// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-06-17
// FILE DESCRIPTION: Abstraction over MSAL token acquisition. Decouples
//   typed API clients and message handlers from the concrete MSAL implementation
//   to enable testability via MockHttpMessageHandler without mocking MSAL directly.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §1.0
// ============================================================

using System.Threading;
using System.Threading.Tasks;

namespace PlantSafety.UI.Infrastructure.Auth;

/// <summary>
/// Abstracts MSAL token acquisition for the PlantSafety API scope.
/// Implementations must handle silent-first, interactive-fallback, and sign-out.
/// </summary>
public interface IMsalTokenProvider
{
    /// <summary>
    /// Acquires a valid bearer access token, using silent cache acquisition first.
    /// </summary>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns>A non-empty access token string.</returns>
    Task<string> AcquireTokenAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears all cached accounts and tokens, requiring re-authentication on next call.
    /// </summary>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    Task SignOutAsync(CancellationToken cancellationToken = default);
}

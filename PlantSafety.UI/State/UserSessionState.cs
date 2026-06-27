// ============================================================
// AUTHOR: PlantSafety Platform
// CREATED: 2026-06-17
// FILE DESCRIPTION: Singleton state container for the authenticated user's
//   identity and claims. Raised via OnChange event so all subscribed components
//   re-render on sign-in, sign-out, or token refresh without cascading values.
// SchemaVersion: 2026.1
// Additive-only evolution — see Enterprise Architectural Rule Handbook §XI
// ============================================================

using System;

using PlantSafety.UI.Models.Auth;

namespace PlantSafety.UI.State;

/// <summary>
/// Singleton state container for the current authenticated user session.
/// Injected via <c>@inject UserSessionState</c>; never passed as a cascading value.
/// </summary>
public sealed class UserSessionState
{
    private UserClaimsDto _user = UserClaimsDto.Anonymous;

    /// <summary>Gets the claims for the currently authenticated user.</summary>
    public UserClaimsDto User => _user;

    /// <summary>Gets a value indicating whether a user is currently authenticated.</summary>
    public bool IsAuthenticated => _user.IsAuthenticated;

    /// <summary>
    /// Raised whenever authentication state changes. Components MUST subscribe in
    /// OnInitialized and unsubscribe in Dispose to prevent memory leaks.
    /// </summary>
    public event Action? OnChange;

    /// <summary>
    /// Updates the session with the freshly authenticated user claims and notifies
    /// all subscribed components to re-render.
    /// </summary>
    /// <param name="claims">The claims extracted from the Azure AD ID token.</param>
    public void SetUser(UserClaimsDto claims)
    {
        _user = claims ?? throw new ArgumentNullException(nameof(claims));
        NotifyStateChanged();
    }

    /// <summary>
    /// Clears the authenticated user, reverting to <see cref="UserClaimsDto.Anonymous"/>.
    /// Called on sign-out.
    /// </summary>
    public void ClearUser()
    {
        _user = UserClaimsDto.Anonymous;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}

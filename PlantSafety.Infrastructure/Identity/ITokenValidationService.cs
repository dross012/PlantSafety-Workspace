// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Identity
//  Layer:          Infrastructure
//  File:           ITokenValidationService.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    Abstraction over Azure AD / Entra ID token validation.
//                  Additive-only evolution.
// ============================================================================
using System.Security.Claims;

namespace PlantSafety.Infrastructure.Identity;

/// <summary>Abstraction over Azure AD / Entra ID bearer token validation.</summary>
public interface ITokenValidationService
{
    /// <summary>
    /// Validates <paramref name="bearerToken"/> and returns the <see cref="ClaimsPrincipal"/>
    /// on success, or throws on validation failure.
    /// </summary>
    Task<ClaimsPrincipal> ValidateAsync(string bearerToken, CancellationToken cancellationToken = default);
}

// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Abstractions for the EnterpriseFabric module.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.EnterpriseFabric.Abstractions;

/// <summary>
/// Bridges the Application layer with the external identity provider (Azure AD B2C / Entra).
/// Implemented in Infrastructure; never call this from domain logic.
/// </summary>
public interface IIdentityProviderService
{
    /// <summary>Provisions the user account in the identity provider. Returns the external user object identifier.</summary>
    Task<string> ProvisionUserAsync(
        Guid tenantId,
        string email,
        string firstName,
        string lastName,
        IReadOnlyList<string> roles,
        CancellationToken cancellationToken);

    /// <summary>Deactivates the user account in the identity provider.</summary>
    Task DeactivateUserAsync(Guid tenantId, string externalUserId, CancellationToken cancellationToken);

    /// <summary>Assigns the given role to the user in the identity provider.</summary>
    Task AssignRoleAsync(
        Guid tenantId,
        string externalUserId,
        string role,
        CancellationToken cancellationToken);
}

/// <summary>Validates that a set of role names are known system roles for the tenant.</summary>
public interface IRoleValidationService
{
    /// <summary>Returns <c>true</c> when all provided role names are valid for the tenant.</summary>
    Task<bool> AreValidRolesAsync(
        Guid tenantId,
        IReadOnlyList<string> roles,
        CancellationToken cancellationToken);
}

// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Validation constants specific to the EnterpriseFabric module.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.EnterpriseFabric.Validation;

/// <summary>Validation constraint constants for EnterpriseFabric features.</summary>
public static class EnterpriseFabricValidationConstants
{
    /// <summary>Maximum email address length (RFC 5321).</summary>
    public const int MaxEmailLength = 320;

    /// <summary>Maximum first name length.</summary>
    public const int MaxFirstNameLength = 100;

    /// <summary>Maximum last name length.</summary>
    public const int MaxLastNameLength = 100;

    /// <summary>Maximum role name length.</summary>
    public const int MaxRoleNameLength = 100;

    /// <summary>Maximum number of roles per user.</summary>
    public const int MaxRolesPerUser = 10;

    /// <summary>Maximum number of site assignments per user.</summary>
    public const int MaxSitesPerUser = 50;
}

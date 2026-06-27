// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Module-scoped constants for the EnterpriseFabric module.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.EnterpriseFabric.Constants;

/// <summary>Module-scoped constants for EnterpriseFabric.</summary>
public static class EnterpriseFabricModuleConstants
{
    /// <summary>Canonical module name.</summary>
    public const string ModuleName = "EnterpriseFabric";

    /// <summary>SQL schema name.</summary>
    public const string SqlSchema = "EnterpriseFabric";

    /// <summary>Service Bus topic name.</summary>
    public const string ServiceBusTopic = "enterprise-fabric";

    /// <summary>Cache key prefix for user profiles.</summary>
    public const string UserCachePrefix = "users";

    /// <summary>User cache expiry in minutes.</summary>
    public const int UserCacheExpiryMinutes = 5;

    /// <summary>Maximum number of roles assignable to a single user.</summary>
    public const int MaxRolesPerUser = 10;

    /// <summary>Maximum number of sites assignable to a single user.</summary>
    public const int MaxSitesPerUser = 50;
}

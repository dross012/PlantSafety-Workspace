// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Module-scoped constants for the Compliance module.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.Compliance.Constants;

/// <summary>Module-scoped constants for Compliance.</summary>
public static class ComplianceModuleConstants
{
    /// <summary>Canonical module name.</summary>
    public const string ModuleName = "Compliance";

    /// <summary>SQL schema name.</summary>
    public const string SqlSchema = "Compliance";

    /// <summary>Service Bus topic name.</summary>
    public const string ServiceBusTopic = "compliance";

    /// <summary>Cache key prefix for permits.</summary>
    public const string PermitCachePrefix = "permits";

    /// <summary>Cache key prefix for MOC records.</summary>
    public const string MocCachePrefix = "moc";

    /// <summary>Default permit cache expiry in minutes.</summary>
    public const int PermitCacheExpiryMinutes = 5;

    /// <summary>Valid permit types.</summary>
    public static readonly IReadOnlyList<string> PermitTypes =
        ["HotWork", "ColdWork", "ConfinedSpace", "Electrical", "LockoutTagout"];

    /// <summary>Valid permit statuses.</summary>
    public static readonly IReadOnlyList<string> PermitStatuses =
        ["Draft", "PendingApproval", "Active", "Suspended", "Closed", "Expired"];

    /// <summary>Valid MOC change types.</summary>
    public static readonly IReadOnlyList<string> ChangeTypes = ["Permanent", "Temporary", "Emergency"];

    /// <summary>Valid MOC risk levels.</summary>
    public static readonly IReadOnlyList<string> RiskLevels = ["Low", "Medium", "High", "Critical"];
}

// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Module-scoped constants for the Operational module.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.Operational.Constants;

/// <summary>Module-scoped constants for Operational.</summary>
public static class OperationalModuleConstants
{
    /// <summary>Canonical module name.</summary>
    public const string ModuleName = "Operational";

    /// <summary>SQL schema name.</summary>
    public const string SqlSchema = "Operational";

    /// <summary>Service Bus topic name.</summary>
    public const string ServiceBusTopic = "operational";

    /// <summary>Cache key prefix for training records.</summary>
    public const string TrainingCachePrefix = "training";

    /// <summary>Valid training delivery methods.</summary>
    public static readonly IReadOnlyList<string> TrainingDeliveryMethods = ["InPerson", "Online", "OJT"];

    /// <summary>Valid training statuses.</summary>
    public static readonly IReadOnlyList<string> TrainingStatuses =
        ["Assigned", "InProgress", "Completed", "Failed", "Expired"];

    /// <summary>Risk score thresholds for RiskRating calculation (L × C).</summary>
    public const int LowRiskMax = 4;
    /// <summary>Medium risk upper bound.</summary>
    public const int MediumRiskMax = 9;
    /// <summary>High risk upper bound.</summary>
    public const int HighRiskMax = 14;
    /// <summary>VeryHigh risk upper bound; above this is Extreme.</summary>
    public const int VeryHighRiskMax = 19;
}

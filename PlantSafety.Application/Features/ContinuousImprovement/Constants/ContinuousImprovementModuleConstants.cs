// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Module-scoped constants for the ContinuousImprovement module.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.ContinuousImprovement.Constants;

/// <summary>Module-scoped constants for ContinuousImprovement.</summary>
public static class ContinuousImprovementModuleConstants
{
    /// <summary>Canonical module name.</summary>
    public const string ModuleName = "ContinuousImprovement";

    /// <summary>SQL schema name.</summary>
    public const string SqlSchema = "ContinuousImprovement";

    /// <summary>Service Bus topic name.</summary>
    public const string ServiceBusTopic = "continuous-improvement";

    /// <summary>Cache key prefix for CAPA records.</summary>
    public const string CapaCachePrefix = "capa";

    /// <summary>Valid CAPA action types.</summary>
    public static readonly IReadOnlyList<string> ActionTypes = ["Corrective", "Preventive"];

    /// <summary>Valid CAPA priority levels.</summary>
    public static readonly IReadOnlyList<string> Priorities = ["Low", "Medium", "High", "Critical"];

    /// <summary>Valid CAPA statuses.</summary>
    public static readonly IReadOnlyList<string> Statuses = ["Open", "InProgress", "PendingVerification", "Closed", "Overdue"];
}

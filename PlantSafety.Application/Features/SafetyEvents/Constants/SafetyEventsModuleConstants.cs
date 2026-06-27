// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Module-scoped constants for the SafetyEvents module.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.SafetyEvents.Constants;

/// <summary>Module-scoped constants for SafetyEvents.</summary>
public static class SafetyEventsModuleConstants
{
    /// <summary>Canonical module name used in event type prefixes and routing keys.</summary>
    public const string ModuleName = "SafetyEvents";

    /// <summary>SQL schema name for all SafetyEvents tables and stored procedures.</summary>
    public const string SqlSchema = "SafetyEvents";

    /// <summary>Service Bus topic name for SafetyEvents integration events.</summary>
    public const string ServiceBusTopic = "safety-events";

    /// <summary>Maximum allowed export record count; enforced by the export job handler.</summary>
    public const int MaxExportRecords = 10_000;

    /// <summary>Cache key prefix for incident records.</summary>
    public const string IncidentCachePrefix = "incidents";

    /// <summary>Cache key prefix for near miss records.</summary>
    public const string NearMissCachePrefix = "near-misses";

    /// <summary>Cache key prefix for observation records.</summary>
    public const string ObservationCachePrefix = "observations";

    /// <summary>Cache key prefix for RCA records.</summary>
    public const string RcaCachePrefix = "rcas";

    /// <summary>Default cache expiry in minutes for single-record queries.</summary>
    public const int DefaultCacheExpiryMinutes = 5;

    /// <summary>Incident severity levels in ascending order of impact.</summary>
    public static readonly IReadOnlyList<string> IncidentSeverities =
        ["Minor", "Moderate", "Serious", "Critical"];

    /// <summary>Near miss hazard categories.</summary>
    public static readonly IReadOnlyList<string> HazardCategories =
        ["Chemical", "Mechanical", "Electrical", "Ergonomic", "Biological", "Radiation", "Thermal", "Other"];
}

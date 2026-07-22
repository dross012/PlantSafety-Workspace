// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: SafetyPolicies module registration marker and constants. All commands, queries, and handlers for this module live under PlantSafety.Application.SafetyPolicies.
// SchemaVersion: 1

namespace PlantSafety.Application.SafetyPolicies;

/// <summary>
/// Module marker and constants for the SafetyPolicies bounded context.
/// </summary>
public static class SafetyPoliciesModule
{
    /// <summary>The canonical module name used in event types, error codes, and log envelopes.</summary>
    public const string ModuleName = "SafetyPolicies";

    /// <summary>The SQL schema name owned by this module.</summary>
    public const string SqlSchema = "SafetyPolicies";

    /// <summary>Canonical integration event type prefix for this module.</summary>
    public const string EventTypePrefix = "SafetyPolicies";

    /// <summary>Permissions for this module.</summary>
    public static class Permissions
    {
        /// <summary>Permission to create a safetypolicy record.</summary>
        public const string CreateSafetyPolicy = "SafetyPolicies.SafetyPolicys.Create";

        /// <summary>Permission to view safetypolicy records.</summary>
        public const string ViewSafetyPolicy = "SafetyPolicies.SafetyPolicys.View";

        /// <summary>Permission to update safetypolicy records.</summary>
        public const string UpdateSafetyPolicy = "SafetyPolicies.SafetyPolicys.Update";
    }
}
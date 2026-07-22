// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: RiskManagement module registration marker and constants. All commands, queries, and handlers for this module live under PlantSafety.Application.RiskManagement.
// SchemaVersion: 1

namespace PlantSafety.Application.RiskManagement;

/// <summary>
/// Module marker and constants for the RiskManagement bounded context.
/// </summary>
public static class RiskManagementModule
{
    /// <summary>The canonical module name used in event types, error codes, and log envelopes.</summary>
    public const string ModuleName = "RiskManagement";

    /// <summary>The SQL schema name owned by this module.</summary>
    public const string SqlSchema = "RiskManagement";

    /// <summary>Canonical integration event type prefix for this module.</summary>
    public const string EventTypePrefix = "RiskManagement";

    /// <summary>Permissions for this module.</summary>
    public static class Permissions
    {
        /// <summary>Permission to create a risk record.</summary>
        public const string CreateRisk = "RiskManagement.Risks.Create";

        /// <summary>Permission to view risk records.</summary>
        public const string ViewRisk = "RiskManagement.Risks.View";

        /// <summary>Permission to update risk records.</summary>
        public const string UpdateRisk = "RiskManagement.Risks.Update";
    }
}
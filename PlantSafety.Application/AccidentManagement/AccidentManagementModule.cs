// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: AccidentManagement module registration marker and constants. All commands, queries, and handlers for this module live under PlantSafety.Application.AccidentManagement.
// SchemaVersion: 1

namespace PlantSafety.Application.AccidentManagement;

/// <summary>
/// Module marker and constants for the AccidentManagement bounded context.
/// </summary>
public static class AccidentManagementModule
{
    /// <summary>The canonical module name used in event types, error codes, and log envelopes.</summary>
    public const string ModuleName = "AccidentManagement";

    /// <summary>The SQL schema name owned by this module.</summary>
    public const string SqlSchema = "AccidentManagement";

    /// <summary>Canonical integration event type prefix for this module.</summary>
    public const string EventTypePrefix = "AccidentManagement";

    /// <summary>Permissions for this module.</summary>
    public static class Permissions
    {
        /// <summary>Permission to create a accident record.</summary>
        public const string CreateAccident = "AccidentManagement.Accidents.Create";

        /// <summary>Permission to view accident records.</summary>
        public const string ViewAccident = "AccidentManagement.Accidents.View";

        /// <summary>Permission to update accident records.</summary>
        public const string UpdateAccident = "AccidentManagement.Accidents.Update";
    }
}
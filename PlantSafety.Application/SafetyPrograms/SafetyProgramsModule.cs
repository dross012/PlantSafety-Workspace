// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-03
// FILE DESCRIPTION: SafetyPrograms module registration marker and constants. All commands,
//   queries, and handlers for this module live under PlantSafety.Application.SafetyPrograms.
//   SafetyPrograms is a thin composition layer over the existing SafetyPolicies,
//   SafetyProcedures, and SafetyCulture modules -- see docs/Phase5/ui-gaps-scoping.md
//   (section 8) for the scoping decision.
// SchemaVersion: 1

namespace PlantSafety.Application.SafetyPrograms;

/// <summary>Module marker and constants for the SafetyPrograms bounded context.</summary>
public static class SafetyProgramsModule
{
    /// <summary>The canonical module name used in event types, error codes, and log envelopes.</summary>
    public const string ModuleName = "SafetyPrograms";

    /// <summary>The SQL schema name owned by this module.</summary>
    public const string SqlSchema = "SafetyPrograms";

    /// <summary>Canonical integration event type prefix for this module.</summary>
    public const string EventTypePrefix = "SafetyPrograms";

    /// <summary>Permissions for this module.</summary>
    public static class Permissions
    {
        /// <summary>Permission to create a safety program.</summary>
        public const string CreateSafetyProgram = "SafetyPrograms.SafetyProgram.Create";

        /// <summary>Permission to view safety programs.</summary>
        public const string ViewSafetyProgram = "SafetyPrograms.SafetyProgram.View";

        /// <summary>Permission to change a safety program's lifecycle status.</summary>
        public const string ChangeSafetyProgramStatus = "SafetyPrograms.SafetyProgram.ChangeStatus";
    }
}

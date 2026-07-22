// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Checklists module registration marker and constants. All commands, queries, and handlers for this module live under PlantSafety.Application.Checklists.
// SchemaVersion: 1

namespace PlantSafety.Application.Checklists;

/// <summary>
/// Module marker and constants for the Checklists bounded context.
/// </summary>
public static class ChecklistsModule
{
    /// <summary>The canonical module name used in event types, error codes, and log envelopes.</summary>
    public const string ModuleName = "Checklists";

    /// <summary>The SQL schema name owned by this module.</summary>
    public const string SqlSchema = "Checklists";

    /// <summary>Canonical integration event type prefix for this module.</summary>
    public const string EventTypePrefix = "Checklists";

    /// <summary>Permissions for this module.</summary>
    public static class Permissions
    {
        /// <summary>Permission to create a checklist template.</summary>
        public const string CreateChecklistTemplate = "Checklists.ChecklistTemplate.Create";

        /// <summary>Permission to view checklist templates.</summary>
        public const string ViewChecklistTemplate = "Checklists.ChecklistTemplate.View";

        /// <summary>Permission to record a checklist completion.</summary>
        public const string CreateChecklistCompletion = "Checklists.ChecklistCompletion.Create";

        /// <summary>Permission to view checklist completions.</summary>
        public const string ViewChecklistCompletion = "Checklists.ChecklistCompletion.View";
    }
}
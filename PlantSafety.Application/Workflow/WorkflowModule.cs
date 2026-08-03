// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-02
// FILE DESCRIPTION: Workflow module registration marker and constants. All commands, queries, and
//   handlers for this module live under PlantSafety.Application.Workflow. Covers workflow
//   definitions (approvals, escalations, notifications, multi-step processes) and their runtime
//   instances (Phase 5 Automation and workflow engine).
// SchemaVersion: 1

namespace PlantSafety.Application.Workflow;

/// <summary>Module marker and constants for the Workflow bounded context.</summary>
public static class WorkflowModule
{
    /// <summary>The canonical module name used in event types, error codes, and log envelopes.</summary>
    public const string ModuleName = "Workflow";

    /// <summary>The SQL schema name owned by this module.</summary>
    public const string SqlSchema = "Workflow";

    /// <summary>Canonical integration event type prefix for this module.</summary>
    public const string EventTypePrefix = "Workflow";

    /// <summary>Permissions for this module.</summary>
    public static class Permissions
    {
        /// <summary>Permission to author and activate workflow definitions.</summary>
        public const string ManageDefinitions = "Workflow.Definitions.Manage";

        /// <summary>Permission to view workflow definitions.</summary>
        public const string ViewDefinitions = "Workflow.Definitions.View";

        /// <summary>Permission to start a new workflow instance.</summary>
        public const string StartInstance = "Workflow.Instances.Start";

        /// <summary>Permission to view workflow instances and their history.</summary>
        public const string ViewInstances = "Workflow.Instances.View";

        /// <summary>Permission to record a human approval/rejection decision on a workflow step.</summary>
        public const string DecideStep = "Workflow.Instances.DecideStep";

        /// <summary>Permission to escalate, retry, or cancel a workflow instance.</summary>
        public const string ManageInstance = "Workflow.Instances.Manage";
    }
}

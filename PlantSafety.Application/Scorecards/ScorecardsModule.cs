// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-07-20
// FILE DESCRIPTION: Scorecards module registration marker and constants. All commands, queries, and handlers for this module live under PlantSafety.Application.Scorecards.
// SchemaVersion: 1

namespace PlantSafety.Application.Scorecards;

/// <summary>
/// Module marker and constants for the Scorecards bounded context.
/// </summary>
public static class ScorecardsModule
{
    /// <summary>The canonical module name used in event types, error codes, and log envelopes.</summary>
    public const string ModuleName = "Scorecards";

    /// <summary>The SQL schema name owned by this module.</summary>
    public const string SqlSchema = "Scorecards";

    /// <summary>Canonical integration event type prefix for this module.</summary>
    public const string EventTypePrefix = "Scorecards";

    /// <summary>Permissions for this module.</summary>
    public static class Permissions
    {
        /// <summary>Permission to create a scorecard record.</summary>
        public const string CreateScorecard = "Scorecards.Scorecards.Create";

        /// <summary>Permission to view scorecard records.</summary>
        public const string ViewScorecard = "Scorecards.Scorecards.View";

        /// <summary>Permission to update scorecard records.</summary>
        public const string UpdateScorecard = "Scorecards.Scorecards.Update";
    }
}
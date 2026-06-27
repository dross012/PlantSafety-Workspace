// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: EnterpriseFabric module registration marker and constants.
//   Covers: AI Governance, AI Training, AI Investigation, Data Lake, Predictive Safety,
//           Digital Twin, Sustainability, Environmental Health, Governance & Access Control.
//   All commands, queries, and handlers for this module are in Features/EnterpriseFabric.
// SchemaVersion: 1
// Additive-only evolution: new constants may be added; no existing constants may be renamed or removed.

namespace PlantSafety.Application.EnterpriseFabric;

/// <summary>
/// Module marker and constants for the EnterpriseFabric bounded context.
/// Covers AI governance, intelligence modules, digital twin, sustainability, and environmental health.
/// This module consumes data from all other modules via integration events; it publishes intelligence insights.
/// </summary>
public static class EnterpriseFabricModule
{
    /// <summary>The canonical module name used in event types, error codes, and log envelopes.</summary>
    public const string ModuleName = "EnterpriseFabric";

    /// <summary>The SQL schema name owned by this module.</summary>
    public const string SqlSchema = "EnterpriseFabric";

    /// <summary>Canonical integration event type prefix for this module.</summary>
    public const string EventTypePrefix = "EnterpriseFabric";

    /// <summary>Permissions for this module.</summary>
    public static class Permissions
    {
        /// <summary>Permission to access AI governance controls.</summary>
        public const string ManageAIGovernance = "EnterpriseFabric.AIGovernance.Manage";

        /// <summary>Permission to view digital twin data.</summary>
        public const string ViewDigitalTwin = "EnterpriseFabric.DigitalTwin.View";

        /// <summary>Permission to access the data lake.</summary>
        public const string AccessDataLake = "EnterpriseFabric.DataLake.Access";

        /// <summary>Permission to view sustainability reports.</summary>
        public const string ViewSustainability = "EnterpriseFabric.Sustainability.View";

        /// <summary>Permission to view environmental health data.</summary>
        public const string ViewEnvironmentalHealth = "EnterpriseFabric.EnvironmentalHealth.View";
    }
}

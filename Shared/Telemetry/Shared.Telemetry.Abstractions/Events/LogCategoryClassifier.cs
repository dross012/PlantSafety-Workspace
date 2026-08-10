// ============================================================================
//  Project:        Shared.Telemetry
//  Module:         Abstractions.Events
//  File:           LogCategoryClassifier.cs
//  Created:        2026-08-09
//  Description:    Deterministically classifies a module/route-segment
//                  name (e.g. GlobalErrorHandler's ModuleFromPath output)
//                  into a LogCategory, so callers don't hand-roll their
//                  own string comparisons.
// ============================================================================
namespace Shared.Telemetry.Abstractions.Events;

/// <summary>Classifies a module name into a LogCategory.</summary>
public static class LogCategoryClassifier
{
    // Route segments for every AI.* (Path E) engine console, as extracted by
    // PlantSafety.UI.Api's GlobalErrorHandler.ModuleFromPath from "api/v1/ui/engines/<name>/...".
    private static readonly HashSet<string> AiEngineModules =
    [
        "reasoning", "hazard-understanding", "workflow-automation", "regulatory-interpretation",
        "severity-assessment", "predictive-optimization", "digital-twin-learning", "safety-copilot",
        "enterprise-orchestration",
    ];

    // Route segments for every Enterprise.* (Path D) engine/module.
    private static readonly HashSet<string> EnterpriseEngineModules =
    [
        "datalake", "graph", "enterprise-dashboard", "rules-engine", "workflow-orchestration",
        "digital-twin", "predictive-modeling", "integration",
    ];

    /// <summary>Classifies the given module name (case-insensitive).</summary>
    public static LogCategory Classify(string module)
    {
        string normalized = module.Trim().ToLowerInvariant();
        if (normalized is "facilities" or "facility")
        {
            return LogCategory.Facility;
        }

        if (normalized is "employees" or "employee")
        {
            return LogCategory.Employee;
        }

        if (normalized is "machines" or "machine")
        {
            return LogCategory.Machine;
        }

        if (normalized is "workflow-engine" or "workflowengine")
        {
            return LogCategory.WorkflowEngine;
        }

        if (normalized == "audit")
        {
            return LogCategory.Audit;
        }

        if (AiEngineModules.Contains(normalized))
        {
            return LogCategory.AiEngine;
        }

        if (EnterpriseEngineModules.Contains(normalized))
        {
            return LogCategory.EnterpriseEngine;
        }

        return LogCategory.Infrastructure;
    }
}

// ============================================================================
//  Project:        PlantSafety.UI.Api
//  Module:         Middleware
//  File:           RequestModuleResolver.cs
//  Created:        2026-08-09
//  Description:    Phase 3.3 deliverable: extracted from GlobalErrorHandler's
//                  original ModuleFromPath so CorrelationIdMiddleware's
//                  request-count metric can tag by the same fixed module
//                  name (e.g. "facilities", "hazard-understanding") that
//                  GlobalErrorHandler already uses for error/log tagging --
//                  never a route parameter value (facility id, employee id),
//                  which would otherwise blow up metric tag cardinality.
// ============================================================================
namespace PlantSafety.UI.Api.Middleware;

/// <summary>Resolves the fixed module name a request path belongs to, for logging and metrics tags.</summary>
internal static class RequestModuleResolver
{
    /// <summary>Resolves the module name from a request path, e.g. "/api/v1/ui/facilities/{id}" -> "facilities".</summary>
    public static string Resolve(string path)
    {
        string[] segments = path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        int uiIndex = Array.IndexOf(segments, "ui");
        if (uiIndex < 0 || uiIndex + 1 >= segments.Length)
        {
            return segments.Length > 0 ? segments[^1] : "PlantSafety.UI.Api";
        }

        // "api/v1/ui/engines/<engine-name>/..." -- the meaningful module is the engine
        // name, not the generic "engines" grouping segment.
        return segments[uiIndex + 1] == "engines" && uiIndex + 2 < segments.Length
            ? segments[uiIndex + 2]
            : segments[uiIndex + 1];
    }
}

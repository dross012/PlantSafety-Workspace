// ============================================================================
// File: InspectionActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for Inspection endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(InspectionActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows Inspection-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for Inspection-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Compliance.Procedures.Inspection.Filters;

/// <summary>
/// Optional action filter for Inspection. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class InspectionActionFilter : IActionFilter
{
    private readonly ILogger<InspectionActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="InspectionActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public InspectionActionFilter(ILogger<InspectionActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("Inspection action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}
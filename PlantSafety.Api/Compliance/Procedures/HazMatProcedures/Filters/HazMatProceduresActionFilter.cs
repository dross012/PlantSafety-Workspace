// ============================================================================
// File: HazMatProceduresActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for HazMatProcedures endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(HazMatProceduresActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows HazMatProcedures-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for HazMatProcedures-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Compliance.Procedures.HazMatProcedures.Filters;

/// <summary>
/// Optional action filter for HazMatProcedures. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class HazMatProceduresActionFilter : IActionFilter
{
    private readonly ILogger<HazMatProceduresActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="HazMatProceduresActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public HazMatProceduresActionFilter(ILogger<HazMatProceduresActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("HazMatProcedures action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}
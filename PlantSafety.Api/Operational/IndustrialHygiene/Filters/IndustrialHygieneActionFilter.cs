// ============================================================================
// File: IndustrialHygieneActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for IndustrialHygiene endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(IndustrialHygieneActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows IndustrialHygiene-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for IndustrialHygiene-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Operational.IndustrialHygiene.Filters;

/// <summary>
/// Optional action filter for IndustrialHygiene. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class IndustrialHygieneActionFilter : IActionFilter
{
    private readonly ILogger<IndustrialHygieneActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="IndustrialHygieneActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public IndustrialHygieneActionFilter(ILogger<IndustrialHygieneActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("IndustrialHygiene action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}
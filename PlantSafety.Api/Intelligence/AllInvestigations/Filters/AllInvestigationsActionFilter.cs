// ============================================================================
// File: AllInvestigationsActionFilter.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: Optional per-sub-area action filter for AllInvestigations endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(AllInvestigationsActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows AllInvestigations-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for AllInvestigations-specific validation or enrichment.
//
// ============================================================================
namespace PlantSafety.Api.Intelligence.AllInvestigations.Filters;

/// <summary>
/// Optional action filter for AllInvestigations. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class AllInvestigationsActionFilter : IActionFilter
{
    private readonly ILogger<AllInvestigationsActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="AllInvestigationsActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public AllInvestigationsActionFilter(ILogger<AllInvestigationsActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("AllInvestigations action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}
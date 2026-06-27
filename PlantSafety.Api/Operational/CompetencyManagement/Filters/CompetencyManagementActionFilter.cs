// ============================================================================
// File: CompetencyManagementActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for CompetencyManagement endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(CompetencyManagementActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows CompetencyManagement-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for CompetencyManagement-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Operational.CompetencyManagement.Filters;

/// <summary>
/// Optional action filter for CompetencyManagement. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class CompetencyManagementActionFilter : IActionFilter
{
    private readonly ILogger<CompetencyManagementActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="CompetencyManagementActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public CompetencyManagementActionFilter(ILogger<CompetencyManagementActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("CompetencyManagement action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}
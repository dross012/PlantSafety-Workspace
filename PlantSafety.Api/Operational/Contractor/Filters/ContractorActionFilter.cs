// ============================================================================
// File: ContractorActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: Optional per-sub-area action filter for Contractor endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(ContractorActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows Contractor-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for Contractor-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Operational.Contractor.Filters;

/// <summary>
/// Optional action filter for Contractor. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class ContractorActionFilter : IActionFilter
{
    private readonly ILogger<ContractorActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="ContractorActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public ContractorActionFilter(ILogger<ContractorActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("Contractor action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}
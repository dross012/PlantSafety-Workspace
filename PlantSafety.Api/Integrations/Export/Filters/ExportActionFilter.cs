// ============================================================================
// File: ExportActionFilter.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: Optional per-sub-area action filter for Integrations.Export endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(ExportActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows Integrations.Export-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for Integrations.Export-specific validation or enrichment.
//
// ============================================================================

namespace PlantSafety.Api.Integrations.Export.Filters;

/// <summary>
/// Optional action filter for Integrations.Export. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class ExportActionFilter : IActionFilter
{
    private readonly ILogger<ExportActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="ExportActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public ExportActionFilter(ILogger<ExportActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("Integrations.Export action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}

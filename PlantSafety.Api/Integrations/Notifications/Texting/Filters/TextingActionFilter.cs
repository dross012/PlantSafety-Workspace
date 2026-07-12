// ============================================================================
// File: TextingActionFilter.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: Optional per-sub-area action filter for Texting endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(TextingActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows Texting-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for Texting-specific validation or enrichment.
//
// ============================================================================
namespace PlantSafety.Api.Integrations.Notifications.Texting.Filters;

/// <summary>
/// Optional action filter for Texting. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class TextingActionFilter : IActionFilter
{
    private readonly ILogger<TextingActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="TextingActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public TextingActionFilter(ILogger<TextingActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("Texting action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}
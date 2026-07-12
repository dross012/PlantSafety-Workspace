// ============================================================================
// File: EmailActionFilter.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-07-12
// Last Updated: 2026-07-12
// Description: Optional per-sub-area action filter for Email endpoints.
// ============================================================================
//
// Onboarding Notes:
// - Applied via [ServiceFilter(typeof(EmailActionFilter))] on the controller when sub-area-specific pre/post processing is needed.
//
// Rationale:
// - Allows Email-specific request enrichment without polluting the global filter pipeline.
//
// Future Extension Seams:
// - Implement OnActionExecuting for Email-specific validation or enrichment.
//
// ============================================================================
namespace PlantSafety.Api.Integrations.Notifications.Email.Filters;

/// <summary>
/// Optional action filter for Email. Applies sub-area-specific request
/// pre-processing before the controller action executes.
/// </summary>
public sealed class EmailActionFilter : IActionFilter
{
    private readonly ILogger<EmailActionFilter> _logger;

    /// <summary>Initialises a new instance of <see cref="EmailActionFilter"/>.</summary>
    /// <param name="logger">Logger for filter diagnostics.</param>
    public EmailActionFilter(ILogger<EmailActionFilter> logger) => _logger = logger;

    /// <inheritdoc/>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogDebug("Email action filter executing for {Path}", context.HttpContext.Request.Path);
    }

    /// <inheritdoc/>
    public void OnActionExecuted(ActionExecutedContext context) { }
}
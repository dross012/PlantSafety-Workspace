// ============================================================================
// File: PlatformV1Controller.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: V1 platform-level controller exposing version metadata and diagnostic endpoints
//              applicable to the entire PlantSafety.Api surface.
// ============================================================================
//
// Onboarding Notes:
// - Inherits BaseApiController for CorrelationId, TenantId, and ApiResponse factories.
// - Routes: /api/v1/platform/... — distinct from module-specific routes.
//
// Rationale:
// - Provides a stable /api/v1/platform/version endpoint so consumers and monitoring
//   tools can verify the running version without parsing module-specific payloads.
//
// Future Extension Seams:
// - Add /api/v1/platform/features to expose enabled feature flags per tenant.
// - Add /api/v1/platform/status for a structured availability summary.
//
// ============================================================================

using System.Reflection;

namespace PlantSafety.Api.Controllers.V1;

/// <summary>
/// Platform-level V1 controller for version metadata and API-wide diagnostics.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/platform")]
[Authorize]
[Tags("Platform")]
public sealed class PlatformV1Controller : BaseApiController
{
    private readonly ILogger<PlatformV1Controller> _logger;

    /// <summary>Initialises a new instance of <see cref="PlatformV1Controller"/>.</summary>
    /// <param name="logger">Logger for platform diagnostic events.</param>
    public PlatformV1Controller(ILogger<PlatformV1Controller> logger) => _logger = logger;

    /// <summary>Returns the running assembly version and build metadata.</summary>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> wrapping the version response.</returns>
    /// <response code="200">Version information returned successfully.</response>
    /// <response code="401">Missing or invalid JWT Bearer token.</response>
    [HttpGet("version")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<PlatformVersionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public Task<IActionResult> GetVersionAsync(CancellationToken cancellationToken)
    {
        var asm = Assembly.GetExecutingAssembly();
        var version = asm.GetName().Version?.ToString() ?? "unknown";
        var response = new PlatformVersionResponse(version, DateTime.UtcNow);

        _logger.LogDebug("Platform version requested. CorrelationId={CorrelationId}", CorrelationId);

        return Task.FromResult(ApiOk(response));
    }
}

/// <summary>Response payload for the platform version endpoint.</summary>
/// <param name="Version">Assembly version string.</param>
/// <param name="QueriedAtUtc">UTC timestamp of this response.</param>
public sealed record PlatformVersionResponse(string Version, DateTime QueriedAtUtc);

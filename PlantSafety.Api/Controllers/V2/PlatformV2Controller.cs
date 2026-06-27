// ============================================================================
// File: PlatformV2Controller.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: V2 stub controller for platform-level endpoints. Holds the V2 route
//              contract while V1 semantics remain in use.
// ============================================================================
//
// Onboarding Notes:
// - Intentionally minimal until a breaking change is required on the /platform surface.
//
// Rationale:
// - Holds the V2 route contract so consumers can target v2 now and receive V1 semantics
//   until V2 is fully implemented.
//
// Future Extension Seams:
// - Implement enriched version payload (build hash, deployment slot, region) in V2.
//
// ============================================================================

namespace PlantSafety.Api.Controllers.V2;

/// <summary>V2 stub controller for platform-level endpoints.</summary>
[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/platform")]
[Authorize]
[Tags("Platform")]
public sealed class PlatformV2Controller : BaseApiController
{
    private readonly ILogger<PlatformV2Controller> _logger;

    /// <summary>Initialises a new instance of <see cref="PlatformV2Controller"/>.</summary>
    /// <param name="logger">Logger for platform diagnostic events.</param>
    public PlatformV2Controller(ILogger<PlatformV2Controller> logger) => _logger = logger;

    /// <summary>V2 stub — returns not-implemented until V2 platform contract is defined.</summary>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> indicating V2 is not yet implemented.</returns>
    [HttpGet("version")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetVersionAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Platform V2 version endpoint called. CorrelationId={CorrelationId}", CorrelationId);
        return Task.FromResult(ApiOk<object>(new { Message = "Platform V2 endpoint not yet implemented - use v1." }));
    }
}

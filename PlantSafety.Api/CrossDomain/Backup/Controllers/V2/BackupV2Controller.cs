// ============================================================================
// File: BackupV2Controller.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-17
// Last Updated: 2026-06-17
// Description: V2 stub controller for Backup. Maintains a forward-compatible route without breaking V1 consumers.
// ============================================================================
//
// Onboarding Notes:
// - V2 routes co-exist with V1 via Asp.Versioning URL-segment versioning. Intentionally minimal until a breaking change is required.
//
// Rationale:
// - Holds the V2 route contract so consumers can target v2 now and receive V1 semantics until V2 is fully implemented.
//
// Future Extension Seams:
// - Implement distinct V2 behaviour here when the Backup contract needs a breaking change.
//
// ============================================================================

using PlantSafety.Api.CrossDomain.Backup.Contracts.Requests;
using PlantSafety.Api.CrossDomain.Backup.Contracts.Responses;
using PlantSafety.Api.CrossDomain.Backup.Authorization;

namespace PlantSafety.Api.CrossDomain.Backup.Controllers.V2;

/// <summary>V2 stub controller for Backup. Delegates to V1 semantics until a breaking change is required.</summary>
[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/cross-domain/backup")]
[Authorize(Policy = BackupAuthorizationPolicies.Read)]
[Tags("Backup")]
public sealed class BackupV2Controller : ControllerBase
{
    private readonly ISender _sender;

    /// <summary>Initialises a new instance of <see cref="BackupV2Controller"/>.</summary>
    /// <param name="sender">MediatR sender for Application layer dispatch.</param>
    public BackupV2Controller(ISender sender) => _sender = sender;

    /// <summary>V2 read endpoint for Backup. Currently returns a not-implemented stub.</summary>
    /// <param name="request">Query parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Standardised <see cref="ApiResponse{T}"/> with Backup data.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<BackupResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public Task<IActionResult> GetAsync(
        [FromQuery] GetBackupRequest request,
        CancellationToken cancellationToken)
    {
        var correlationId = HttpContext.Items[PlantSafety.Api.Middleware.CorrelationIdMiddleware.ItemsKey]?.ToString()
                            ?? string.Empty;
        IActionResult result = Ok(ApiResponse<BackupResponse>.Fail(
            "Backup V2 endpoint not yet implemented - use v1.", correlationId));
        return Task.FromResult(result);
    }
}
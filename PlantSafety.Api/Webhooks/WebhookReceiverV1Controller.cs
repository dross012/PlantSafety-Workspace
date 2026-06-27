// ============================================================================
// File: WebhookReceiverV1Controller.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: V1 controller that receives inbound webhook POST requests from
//              external systems. Verifies the HMAC-SHA256 signature, deserializes
//              the canonical InboundWebhookPayload envelope, and dispatches a
//              ProcessInboundWebhookCommand to the Application layer.
// ============================================================================
//
// Onboarding Notes:
// - Route: POST api/v1/webhooks/receive
// - Auth: [AllowAnonymous] — this endpoint is secured by HMAC-SHA256 signature
//   verification via WebhookSignatureVerifier, NOT by JWT. The signing secret is
//   shared out-of-band with each external system.
// - The raw request body MUST be read before any JSON deserialization so the
//   HMAC is computed over the exact bytes sent by the external system.
//   EnableBuffering() allows the body stream to be rewound after reading.
// - The controller returns 200 OK immediately on successful dispatch regardless
//   of Application layer processing outcome. Webhook senders expect a fast
//   acknowledgment; actual processing happens asynchronously via the outbox.
// - 401 Unauthorized is returned for invalid signatures (not 403, to avoid
//   leaking information about whether the endpoint exists).
// - Application layer reference:
//     PlantSafety.Application.Integrations.Commands.ProcessInboundWebhookCommand
//     PlantSafety.Application.Integrations.Commands.ProcessInboundWebhookResult
//
// Rationale:
// - The fast-ack + async-process pattern prevents webhook sender timeouts (most
//   senders time out after 5-30 seconds) and decouples inbound event ingestion
//   from Application layer processing latency.
//
// Future Extension Seams:
// - Add per-source routing: different HMAC secrets per registered external system,
//   resolved from the X-Webhook-Source header before verification.
// - Add a GET api/v1/webhooks/ping endpoint for external systems to verify
//   connectivity without sending a real payload.
//
// ============================================================================

using System.Text;
using System.Text.Json;
using PlantSafety.Api.Webhooks.Models;
using PlantSafety.Api.Webhooks.Verification;
using Asp.Versioning;

namespace PlantSafety.Api.Webhooks;

/// <summary>
/// V1 controller that receives and acknowledges inbound webhook payloads from
/// external systems. Secured by HMAC-SHA256 signature verification.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/webhooks")]
public sealed class WebhookReceiverV1Controller : Controllers.BaseApiController
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ISender                   _sender;
    private readonly WebhookSignatureVerifier  _verifier;
    private readonly ILogger<WebhookReceiverV1Controller> _logger;

    /// <summary>Initialises a new instance of <see cref="WebhookReceiverV1Controller"/>.</summary>
    public WebhookReceiverV1Controller(
        ISender                              sender,
        WebhookSignatureVerifier             verifier,
        ILogger<WebhookReceiverV1Controller> logger)
    {
        _sender   = sender;
        _verifier = verifier;
        _logger   = logger;
    }

    /// <summary>
    /// Receives an inbound webhook payload, verifies the HMAC-SHA256 signature,
    /// and dispatches processing to the Application layer.
    /// Returns 200 immediately after successful dispatch (fast-ack pattern).
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// 200 OK on successful receipt and dispatch;
    /// 401 Unauthorized for invalid or missing signatures;
    /// 400 Bad Request for malformed payloads.
    /// </returns>
    [HttpPost("receive")]
    [AllowAnonymous]
    public async Task<IActionResult> ReceiveAsync(CancellationToken ct)
    {
        // Read raw body first so HMAC is computed over the exact wire bytes
        Request.EnableBuffering();
        using var reader  = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
        var rawBody       = await reader.ReadToEndAsync(ct);
        Request.Body.Position = 0;

        var signature = Request.Headers["X-Webhook-Signature"].ToString();

        if (!_verifier.Verify(rawBody, signature))
        {
            _logger.LogWarning(
                "Webhook signature verification failed. Source={Source} CorrelationId={CorrelationId}",
                Request.Headers["X-Webhook-Source"].ToString(),
                CorrelationId);
            return Unauthorized();
        }

        InboundWebhookPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<InboundWebhookPayload>(rawBody, JsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex,
                "Webhook payload deserialization failed. CorrelationId={CorrelationId}", CorrelationId);
            return BadRequest(new { error = "Malformed webhook payload." });
        }

        if (payload is null)
            return BadRequest(new { error = "Empty webhook payload." });

        _logger.LogInformation(
            "Webhook received. EventId={EventId} EventType={EventType} " +
            "Source={Source} TenantId={TenantId} CorrelationId={CorrelationId}",
            payload.EventId, payload.EventType, payload.Source,
            payload.TenantId, CorrelationId);

        var command = new PlantSafety.Application.Integrations.Commands.ProcessInboundWebhookCommand(
            payload.EventId,
            payload.EventType.ToString(),
            payload.Source,
            payload.TenantId,
            payload.SiteId,
            payload.OccurredAt,
            rawBody);

        var result = await _sender.Send(command, ct);

        if (result.IsFailure)
        {
            _logger.LogError(
                "Webhook processing failed. EventId={EventId} Error={Error}",
                payload.EventId, result.Error.Message);
        }

        // Always return 200 — sender does not need to know about downstream failures.
        // Failed events are retried via the outbox pattern in the Infrastructure layer.
        return Ok();
    }
}

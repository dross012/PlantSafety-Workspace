// ============================================================================
// File: SecretRotationWebhookV1Controller.cs
// Project: PlantSafety.Api
// Author: PlantSafety Platform Team
// Created: 2026-08-03
// Description: Phase 6.6 (§6.3 Enterprise Secrets Management) webhook-style endpoint that
//   Azure Key Vault's Event Grid rotation-event subscription could call to notify the
//   platform that a secret was rotated. Actual secret rotation always happens Azure-side;
//   this endpoint only records the notification (via RecordSecretRotationCommand) --
//   secret NAME, rotation timestamp, and triggering context, never a value.
//
//   Handles Event Grid's mandatory subscription-validation handshake
//   (Microsoft.EventGrid.SubscriptionValidationEvent) by echoing the validation code back,
//   per https://learn.microsoft.com/azure/event-grid/webhook-event-delivery.
//
//   Auth: [AllowAnonymous] + a configuration-driven shared-secret header, following the
//   same pattern as WebhookReceiverV1Controller (HMAC-secured, not JWT-secured) --
//   see WebhookAuthorizationPolicies.cs's remarks on why inbound webhook endpoints cannot
//   require interactive JWT auth. NEEDS REAL INFRA: there is no live Key Vault or Event
//   Grid topic in this sandboxed dev environment to actually deliver a signed request here;
//   the shared-secret check is this pass's honest placeholder for a production HMAC/OIDC
//   validation scheme, which should be hardened once a real Event Grid subscription exists.
// ============================================================================
using System.Text.Json;

using PlantSafety.Api.Operations.Secrets.Models;
using PlantSafety.Application.Operations.Secrets.Commands;

namespace PlantSafety.Api.Operations.Secrets.Controllers.V1;

/// <summary>
/// Webhook-style endpoint for Key Vault/Event Grid secret-rotation notifications.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/operations/secrets/rotation-webhook")]
[Tags("Operations.Secrets")]
public sealed class SecretRotationWebhookV1Controller : PlantSafety.Api.Controllers.BaseApiController
{
    private const string SharedSecretHeaderName = "X-Rotation-Webhook-Secret";
    private const string SharedSecretConfigKey = "Secrets:RotationWebhookSharedSecret";

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly ISender _sender;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SecretRotationWebhookV1Controller> _logger;

    /// <summary>Initialises a new instance of <see cref="SecretRotationWebhookV1Controller"/>.</summary>
    public SecretRotationWebhookV1Controller(
        ISender sender,
        IConfiguration configuration,
        ILogger<SecretRotationWebhookV1Controller> logger)
    {
        _sender = sender;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Receives an Event Grid event array. Handles the subscription-validation handshake on
    /// first registration, and dispatches <see cref="RecordSecretRotationCommand"/> for every
    /// real Key Vault secret event delivered afterward.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> ReceiveAsync(CancellationToken cancellationToken)
    {
        var configuredSecret = _configuration[SharedSecretConfigKey];
        if (!string.IsNullOrWhiteSpace(configuredSecret))
        {
            var providedSecret = Request.Headers[SharedSecretHeaderName].ToString();
            if (!string.Equals(providedSecret, configuredSecret, StringComparison.Ordinal))
            {
                _logger.LogWarning("Secret rotation webhook call rejected: missing/incorrect shared-secret header. CorrelationId={CorrelationId}", CorrelationId);
                return Unauthorized();
            }
        }
        else
        {
            _logger.LogWarning(
                "Secret rotation webhook has no '{ConfigKey}' configured -- accepting the call unauthenticated. " +
                "Configure a shared secret (or a stronger scheme) before pointing a real Event Grid subscription at this endpoint.",
                SharedSecretConfigKey);
        }

        using var reader = new StreamReader(Request.Body);
        var rawBody = await reader.ReadToEndAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(rawBody))
        {
            return BadRequest(new { error = "Empty Event Grid payload." });
        }

        using var document = JsonDocument.Parse(rawBody);
        if (document.RootElement.ValueKind != JsonValueKind.Array)
        {
            return BadRequest(new { error = "Expected an Event Grid event array." });
        }

        foreach (var element in document.RootElement.EnumerateArray())
        {
            var eventType = element.TryGetProperty("eventType", out var eventTypeProp) ? eventTypeProp.GetString() : null;

            if (string.Equals(eventType, "Microsoft.EventGrid.SubscriptionValidationEvent", StringComparison.OrdinalIgnoreCase))
            {
                var data = element.TryGetProperty("data", out var dataProp)
                    ? dataProp.Deserialize<EventGridSubscriptionValidationData>(JsonOptions)
                    : null;

                _logger.LogInformation("Secret rotation webhook subscription-validation handshake received. CorrelationId={CorrelationId}", CorrelationId);
                return Ok(new { validationResponse = data?.ValidationCode });
            }

            var payload = element.Deserialize<SecretRotationEventGridPayload>(JsonOptions);
            var secretName = payload?.Data?.ObjectName;

            if (string.IsNullOrWhiteSpace(secretName))
            {
                _logger.LogWarning("Secret rotation webhook received an event with no secret name (ObjectName); ignoring. CorrelationId={CorrelationId}", CorrelationId);
                continue;
            }

            var command = new RecordSecretRotationCommand(
                secretName,
                payload!.EventTime ?? DateTime.UtcNow,
                "EventGrid.WebhookNotification");

            var result = await _sender.Send(command, cancellationToken);

            if (result.IsFailure)
            {
                _logger.LogError("Failed to record secret rotation for {SecretName}. Error={Error} CorrelationId={CorrelationId}", secretName, result.Error.Message, CorrelationId);
            }
        }

        // Event Grid expects a fast 200 acknowledgement regardless of downstream processing.
        return Ok();
    }
}

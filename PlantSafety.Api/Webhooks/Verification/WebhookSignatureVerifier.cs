// ============================================================================
// File: WebhookSignatureVerifier.cs
// Project: PlantSafety.Api
// Author: David (Ted)
// Created: 2026-06-13
// Last Updated: 2026-06-13
// Description: HMAC-SHA256 signature verifier for inbound webhook payloads.
//              Validates the X-Webhook-Signature header against the raw request
//              body using the shared signing secret from configuration.
// ============================================================================
//
// Onboarding Notes:
// - The signing secret is read from configuration key "Webhooks:SigningSecret".
//   In production this key is sourced from Azure Key Vault, never appsettings.json.
// - Expected signature format: "sha256=<hex-encoded HMAC-SHA256 of raw body>".
//   The "sha256=" prefix is stripped before comparison (matches GitHub webhook format).
// - Uses CryptographicOperations.FixedTimeEquals for constant-time comparison to
//   prevent timing side-channel attacks.
// - The verifier is registered as a singleton in ServiceCollectionExtensions
//   (the signing secret does not change per-request).
// - If Webhooks:SigningSecret is missing from configuration, Verify() returns false
//   for all requests (fail-closed). A warning is logged at startup.
//
// Rationale:
// - HMAC-SHA256 over the raw body is the industry-standard webhook authentication
//   pattern (used by GitHub, Stripe, Twilio). It proves the payload originated
//   from a system holding the shared secret and was not tampered in transit.
//
// Future Extension Seams:
// - Add per-source signing secrets (different secret per external system) by
//   resolving the secret based on the "source" field in the payload header before
//   body deserialization.
// - Add timestamp validation: require an X-Webhook-Timestamp header and reject
//   payloads older than 5 minutes to prevent replay attacks.
//
// ============================================================================

using System.Security.Cryptography;
using System.Text;

namespace PlantSafety.Api.Webhooks.Verification;

/// <summary>
/// Validates inbound webhook <c>X-Webhook-Signature</c> headers using HMAC-SHA256.
/// </summary>
public sealed class WebhookSignatureVerifier
{
    private const string SignaturePrefix = "sha256=";
    private const string ConfigKey       = "Webhooks:SigningSecret";

    private readonly byte[]? _signingSecretBytes;
    private readonly ILogger<WebhookSignatureVerifier> _logger;

    /// <summary>
    /// Initialises a new instance of <see cref="WebhookSignatureVerifier"/>.
    /// </summary>
    /// <param name="configuration">Application configuration (reads Webhooks:SigningSecret).</param>
    /// <param name="logger">Logger for configuration warnings.</param>
    public WebhookSignatureVerifier(
        IConfiguration configuration,
        ILogger<WebhookSignatureVerifier> logger)
    {
        _logger = logger;

        var secret = configuration[ConfigKey];
        if (string.IsNullOrWhiteSpace(secret))
        {
            _logger.LogWarning(
                "Webhooks:SigningSecret is not configured. " +
                "All inbound webhook signature verifications will fail.");
            _signingSecretBytes = null;
        }
        else
        {
            _signingSecretBytes = Encoding.UTF8.GetBytes(secret);
        }
    }

    /// <summary>
    /// Verifies that the supplied <paramref name="signature"/> matches the HMAC-SHA256
    /// of <paramref name="rawBody"/> using the configured signing secret.
    /// </summary>
    /// <param name="rawBody">The raw UTF-8 request body string (before JSON deserialization).</param>
    /// <param name="signature">
    /// The value of the <c>X-Webhook-Signature</c> header
    /// (format: <c>sha256=&lt;hex&gt;</c>).
    /// </param>
    /// <returns><c>true</c> if the signature is valid; <c>false</c> otherwise.</returns>
    public bool Verify(string rawBody, string signature)
    {
        if (_signingSecretBytes is null)
            return false;

        if (string.IsNullOrWhiteSpace(signature))
            return false;

        var normalised = signature.StartsWith(SignaturePrefix, StringComparison.OrdinalIgnoreCase)
            ? signature[SignaturePrefix.Length..]
            : signature;

        using var hmac    = new HMACSHA256(_signingSecretBytes);
        var bodyBytes     = Encoding.UTF8.GetBytes(rawBody);
        var computedHash  = hmac.ComputeHash(bodyBytes);
        var computedHex   = Convert.ToHexString(computedHash).ToLowerInvariant();

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedHex),
            Encoding.UTF8.GetBytes(normalised.ToLowerInvariant()));
    }
}

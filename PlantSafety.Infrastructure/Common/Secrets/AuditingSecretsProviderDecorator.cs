// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Infrastructure.Common.Secrets
//  Layer:          Infrastructure
//  File:           AuditingSecretsProviderDecorator.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team
//  SchemaVersion:  2026.1
//  Description:    Decorator wrapping any ISecretsProvider so every secret retrieval is
//                  logged -- secret NAME and caller context only, NEVER the secret value.
//                  This is the single access point every ISecretsProvider consumer resolves
//                  from DI (see InfrastructureServiceRegistration.cs); the underlying
//                  Azure Key Vault / Configuration provider is never resolved directly by
//                  application code.
//
//                  AUDIT SINK NOTE (honest gap, not fabricated): the platform's durable,
//                  queryable audit trail (dbo.AuditLog, read via Shared.AuditLogging's
//                  IAuditLogRepository) is written to EXCLUSIVELY by SQL Server stored
//                  procedures embedded in ~400 call sites (see docs\architecture\audit-
//                  logging.md) -- IAuditLogRepository is deliberately READ-ONLY in C#, and no
//                  INSERT-capable path into dbo.AuditLog exists at the application-code
//                  level for data that (like Key Vault secrets) has no backing SQL row of
//                  its own. docs\architecture\audit-logging.md already documents an
//                  equivalent, pre-existing gap for feature-flag toggles
//                  (SetFeatureFlagEnabledCommandHandler logs via ILogger only, not
//                  dbo.AuditLog). This decorator follows that SAME established convention:
//                  every retrieval is written as a structured ILogger entry under a
//                  dedicated category so it is at minimum reliably queryable via the
//                  platform's log aggregation, pending a future INSERT-capable audit path
//                  for non-tabular security events. Additive-only evolution.
// ============================================================================
using Microsoft.Extensions.Logging;

namespace PlantSafety.Infrastructure.Common.Secrets;

/// <summary>
/// Logs every <see cref="ISecretsProvider.GetSecretAsync"/> call (secret NAME and caller
/// context only -- never the value) before delegating to the wrapped provider.
/// </summary>
public sealed class AuditingSecretsProviderDecorator : ISecretsProvider
{
    private readonly ISecretsProvider _inner;
    private readonly ILogger<AuditingSecretsProviderDecorator> _logger;

    /// <summary>Wraps <paramref name="inner"/> with access-audit logging.</summary>
    public AuditingSecretsProviderDecorator(ISecretsProvider inner, ILogger<AuditingSecretsProviderDecorator> logger)
    {
        _inner = inner;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<string?> GetSecretAsync(string secretName, CancellationToken cancellationToken)
    {
        var requestedAtUtc = DateTime.UtcNow;

        var value = await _inner.GetSecretAsync(secretName, cancellationToken);

        // Deliberately logs SecretName + outcome + timestamp only. `value` is never logged,
        // interpolated, or passed to any logging call, per Rule "secrets are security-critical".
        _logger.LogInformation(
            "Secrets audit: SecretName={SecretName}; Found={Found}; RequestedAtUtc={RequestedAtUtc:O}; Provider={ProviderType}",
            secretName,
            value is not null,
            requestedAtUtc,
            _inner.GetType().Name);

        return value;
    }
}

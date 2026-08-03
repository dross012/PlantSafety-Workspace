// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tenancy.Tenant
//  Layer:          Infrastructure
//  File:           TenantApiKeyGenerator.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 7 §2.1 Automated Tenant Creation)
//  SchemaVersion:  2026.1
//  Description:    ITenantApiKeyGenerator implementation. Generates a cryptographically random
//                  256-bit key (System.Security.Cryptography.RandomNumberGenerator), persists
//                  only its SHA-256 hash (never the plaintext) to [Tenancy].[TenantApiKey], and
//                  returns the plaintext exactly once. Deliberately independent of the
//                  pre-existing ApiKeyAuthFilter/ApiSecurity:ApiKey mechanism -- that validates a
//                  single, platform-wide, Key-Vault-backed key for internal service-to-service/
//                  webhook clients; this is a new, minimal, per-tenant key issuance mechanism
//                  (no such mechanism existed anywhere in the codebase prior to this file,
//                  confirmed by search). Additive-only evolution.
// ============================================================================
using System.Security.Cryptography;
using PlantSafety.Application.Tenancy.Tenant.Abstractions;

namespace PlantSafety.Infrastructure.Tenancy.Tenant.Provisioning;

/// <summary>Generates and stores tenant-scoped API keys.</summary>
public sealed class TenantApiKeyGenerator(ISqlConnectionFactory connectionFactory, DapperCommandExecutor commandExecutor, ILogger<TenantApiKeyGenerator> logger)
    : ITenantApiKeyGenerator
{
    private const int KeyLengthBytes = 32;

    /// <inheritdoc/>
    public async Task<string> GenerateAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var keyBytes = RandomNumberGenerator.GetBytes(KeyLengthBytes);
        var plaintextKey = $"psk_{Convert.ToBase64String(keyBytes).TrimEnd('=').Replace('+', '-').Replace('/', '_')}";
        var keyHash = Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(plaintextKey)));

        using var conn = connectionFactory.CreateConnection();

        var parameters = new StoredProcedureBuilder()
            .AddGuid("TenantApiKeyId", Guid.NewGuid())
            .AddGuid("TenantId", tenantId)
            .AddString("KeyHash", keyHash, 128)
            .AddString("KeyPrefix", plaintextKey[..12], 20)
            .AddDateTime("CreatedAtUtc", DateTime.UtcNow)
            .Build();

        await commandExecutor.ExecuteAsync(conn, "[Tenancy].[usp_TenantApiKey_Insert]", parameters, transaction: null, cancellationToken: cancellationToken);

        logger.LogInformation("TenantProvisioning: primary API key generated for Tenant {TenantId} (prefix {KeyPrefix}).", tenantId, plaintextKey[..12]);

        return plaintextKey;
    }
}

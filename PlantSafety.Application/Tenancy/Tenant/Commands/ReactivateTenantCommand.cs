// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Command to reactivate a Suspended tenant back to Active (Phase 7 §2.3 Tenant
//   Lifecycle Management). Carries the full command envelope per Handbook Rule 2.1.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Tenancy.Tenant.Models;

namespace PlantSafety.Application.Tenancy.Tenant.Commands;

/// <summary>Reactivates a Suspended tenant back to Active.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TargetTenantId">Identity of the tenant to reactivate.</param>
/// <param name="ActorId">Identity of the platform administrator requesting reactivation.</param>
public sealed record ReactivateTenantCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TargetTenantId,
    Guid ActorId) : ICommand<Result<TenantDto>>
{
    /// <summary>Platform-admin, cross-tenant lifecycle operation -- see <see cref="SuspendTenantCommand"/>'s equivalent member remarks for the full rationale.</summary>
    Guid ICommand<Result<TenantDto>>.TenantId => TargetTenantId;

    /// <inheritdoc/>
    Guid? ICommand<Result<TenantDto>>.CausationId => CausationId;

    /// <summary>Builds a command from raw values, generating envelope identifiers where not supplied.</summary>
    public static ReactivateTenantCommand FromDto(
        Guid targetTenantId, Guid actorId, Guid? correlationId = null, Guid? causationId = null, string apiVersionToken = "v1")
        => new(Guid.NewGuid(), correlationId ?? Guid.NewGuid(), causationId ?? Guid.NewGuid(), DateTime.UtcNow, apiVersionToken, targetTenantId, actorId);
}

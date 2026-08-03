// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Command to soft-delete a tenant (Phase 7 §2.3 Tenant Lifecycle Management).
//   Never a hard DELETE, per this codebase's canonical soft-delete pattern -- the row is retained
//   with Status=Deleted/DeletedAtUtc/DeletionReason for audit/legal-hold purposes. Carries the
//   full command envelope per Handbook Rule 2.1.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Tenancy.Tenant.Models;

namespace PlantSafety.Application.Tenancy.Tenant.Commands;

/// <summary>Soft-deletes a tenant.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TargetTenantId">Identity of the tenant to soft-delete.</param>
/// <param name="ActorId">Identity of the platform administrator requesting deletion.</param>
/// <param name="Reason">Human-readable reason for the deletion.</param>
public sealed record DeleteTenantCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TargetTenantId,
    Guid ActorId,
    string Reason) : ICommand<Result<TenantDto>>
{
    /// <summary>Platform-admin, cross-tenant lifecycle operation -- see <see cref="SuspendTenantCommand"/>'s equivalent member remarks for the full rationale.</summary>
    Guid ICommand<Result<TenantDto>>.TenantId => TargetTenantId;

    /// <inheritdoc/>
    Guid? ICommand<Result<TenantDto>>.CausationId => CausationId;

    /// <summary>Builds a command from raw values, generating envelope identifiers where not supplied.</summary>
    public static DeleteTenantCommand FromDto(
        Guid targetTenantId, Guid actorId, string reason, Guid? correlationId = null, Guid? causationId = null, string apiVersionToken = "v1")
        => new(Guid.NewGuid(), correlationId ?? Guid.NewGuid(), causationId ?? Guid.NewGuid(), DateTime.UtcNow, apiVersionToken,
            targetTenantId, actorId, (reason ?? string.Empty).Trim());
}

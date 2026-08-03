// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Command to archive a tenant's data (Phase 7 §2.3 Tenant Lifecycle
//   Management). Marks the Tenant aggregate Archived. SCOPE NOTE: the pre-existing
//   Operations.Archival module (TriggerPurgeCommand/IArchivalRepository, Phase 5/6) is a
//   PLATFORM-OPERATIONS retention/purge pipeline over 5 fixed global targets (ArchiveDB
//   retention jobs) -- it has no per-tenant-scoped archival entry point today (confirmed by
//   reading its command/repository surface), so this command does not force-fit that mechanism.
//   Instead this command records the tenant-level archival decision on the Tenant aggregate
//   itself (a real, auditable state transition raising TenantArchivedEvent) -- the authoritative
//   signal downstream data-movement tooling (a future, genuinely tenant-scoped extension of
//   Operations.Archival, or a dedicated per-tenant archival job) should react to via the outbox,
//   exactly as every other cross-module consumer in this codebase already reacts to domain
//   events rather than being called synchronously inline. Carries the full command envelope per
//   Handbook Rule 2.1.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Tenancy.Tenant.Models;

namespace PlantSafety.Application.Tenancy.Tenant.Commands;

/// <summary>Archives a tenant's data, transitioning the Tenant aggregate to Archived status.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TargetTenantId">Identity of the tenant whose data should be archived.</param>
/// <param name="ActorId">Identity of the platform administrator requesting archival.</param>
public sealed record ArchiveTenantDataCommand(
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
    public static ArchiveTenantDataCommand FromDto(
        Guid targetTenantId, Guid actorId, Guid? correlationId = null, Guid? causationId = null, string apiVersionToken = "v1")
        => new(Guid.NewGuid(), correlationId ?? Guid.NewGuid(), causationId ?? Guid.NewGuid(), DateTime.UtcNow, apiVersionToken, targetTenantId, actorId);
}

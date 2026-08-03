// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Command to suspend an Active tenant (Phase 7 §2.3 Tenant Lifecycle
//   Management), e.g. a billing or compliance hold. Carries the full command envelope per
//   Handbook Rule 2.1.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Tenancy.Tenant.Models;

namespace PlantSafety.Application.Tenancy.Tenant.Commands;

/// <summary>Suspends an Active tenant.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="TargetTenantId">Identity of the tenant to suspend.</param>
/// <param name="ActorId">Identity of the platform administrator requesting suspension.</param>
/// <param name="Reason">Human-readable reason for the suspension.</param>
public sealed record SuspendTenantCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid TargetTenantId,
    Guid ActorId,
    string Reason) : ICommand<Result<TenantDto>>
{
    /// <summary>
    /// This is a platform-admin, cross-tenant lifecycle operation -- the caller's own
    /// <c>IOperationContext.TenantId</c> (their platform-operator tenant) is never the tenant
    /// being suspended, so <see cref="TargetTenantId"/> (not a public property literally named
    /// <c>TenantId</c>) carries the tenant identity, and <see cref="ICommand{TResponse}.TenantId"/>
    /// is satisfied via an EXPLICIT interface implementation -- invisible to
    /// <see cref="Behaviors.Security.TenantIsolationBehavior{TRequest,TResponse}"/>'s public-property
    /// reflection lookup, deliberately opting this command out of per-tenant isolation enforcement
    /// (authorization is instead enforced by <c>TenantAuthorizationPolicies.Manage</c>'s
    /// PlatformAdmin-tier gate on the controller action).
    /// </summary>
    Guid ICommand<Result<TenantDto>>.TenantId => TargetTenantId;

    /// <inheritdoc/>
    Guid? ICommand<Result<TenantDto>>.CausationId => CausationId;

    /// <summary>Builds a command from raw values, generating envelope identifiers where not supplied.</summary>
    public static SuspendTenantCommand FromDto(
        Guid targetTenantId, Guid actorId, string reason, Guid? correlationId = null, Guid? causationId = null, string apiVersionToken = "v1")
        => new(Guid.NewGuid(), correlationId ?? Guid.NewGuid(), causationId ?? Guid.NewGuid(), DateTime.UtcNow, apiVersionToken,
            targetTenantId, actorId, (reason ?? string.Empty).Trim());
}

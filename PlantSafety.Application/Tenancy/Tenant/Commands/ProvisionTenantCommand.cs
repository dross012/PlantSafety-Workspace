// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Command driving the full automated tenant-provisioning pipeline (Phase 7
//   §2.1): create the Tenant record -> initialize RLS/session-context for the new TenantId ->
//   seed default reference data -> create the tenant's initial admin user -> assign roles ->
//   generate an API key -> mark the tenant Active. Carries the full command envelope per
//   Handbook Rule 2.1.
// SchemaVersion: 1

using PlantSafety.Application.Abstractions.Messaging;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Tenancy.Tenant.Models;

namespace PlantSafety.Application.Tenancy.Tenant.Commands;

/// <summary>Provisions a new platform tenant end-to-end.</summary>
/// <param name="CommandId">Unique identifier for this command instance (Uuid7).</param>
/// <param name="CorrelationId">Operation-group correlation identifier.</param>
/// <param name="CausationId">Identifier of the immediate parent command or event.</param>
/// <param name="RequestedAtUtc">UTC instant the command was issued.</param>
/// <param name="ApiVersionToken">API version token the request was issued under.</param>
/// <param name="ActorId">Identity of the platform administrator requesting provisioning.</param>
/// <param name="Name">The new tenant's display name.</param>
/// <param name="Region">Data-residency/deployment region the tenant is pinned to.</param>
/// <param name="PlanCode">Subscription plan reference code (e.g. "Trial", "Standard", "Professional", "Enterprise").</param>
/// <param name="AdminEmail">Email address for the tenant's initial administrative user.</param>
/// <param name="AdminFirstName">First name for the tenant's initial administrative user.</param>
/// <param name="AdminLastName">Last name for the tenant's initial administrative user.</param>
public sealed record ProvisionTenantCommand(
    Guid CommandId,
    Guid CorrelationId,
    Guid CausationId,
    DateTime RequestedAtUtc,
    string ApiVersionToken,
    Guid ActorId,
    string Name,
    string Region,
    string PlanCode,
    string AdminEmail,
    string AdminFirstName,
    string AdminLastName) : ICommand<Result<TenantProvisioningResultDto>>
{
    /// <summary>
    /// Provisioning has no resolved TenantId yet (it is created inside this command) and is
    /// inherently a cross-tenant, platform-admin operation -- the caller's own resolved
    /// <c>IOperationContext.TenantId</c> (their platform-operator tenant) is never the tenant
    /// being provisioned. <see cref="Behaviors.Security.TenantIsolationBehavior{TRequest,TResponse}"/>
    /// reflects for a PUBLIC property literally named <c>TenantId</c> on the concrete request
    /// type and rejects the request if that value does not match the caller's own tenant -- so
    /// this command satisfies <see cref="ICommand{TResponse}"/>'s envelope contract via an
    /// EXPLICIT interface implementation (not a public property), which is invisible to that
    /// reflection lookup, deliberately opting this platform-admin command out of per-tenant
    /// isolation enforcement (authorization is instead enforced by
    /// <c>TenantAuthorizationPolicies.Manage</c>'s PlatformAdmin-tier gate on the controller
    /// action). This mirrors the same technique already used below for <c>CausationId</c>
    /// (Guid vs Guid? type mismatch), repurposed here deliberately rather than accidentally.
    /// </summary>
    Guid ICommand<Result<TenantProvisioningResultDto>>.TenantId => Guid.Empty;

    /// <inheritdoc/>
    Guid? ICommand<Result<TenantProvisioningResultDto>>.CausationId => CausationId;

    /// <summary>Builds a command from raw values, generating envelope identifiers where not supplied.</summary>
    public static ProvisionTenantCommand FromDto(
        Guid actorId, string name, string region, string planCode,
        string adminEmail, string adminFirstName, string adminLastName,
        Guid? correlationId = null, Guid? causationId = null, string apiVersionToken = "v1")
        => new(
            Guid.NewGuid(), correlationId ?? Guid.NewGuid(), causationId ?? Guid.NewGuid(), DateTime.UtcNow, apiVersionToken,
            actorId, (name ?? string.Empty).Trim(), (region ?? string.Empty).Trim(), (planCode ?? string.Empty).Trim(),
            (adminEmail ?? string.Empty).Trim(), (adminFirstName ?? string.Empty).Trim(), (adminLastName ?? string.Empty).Trim());
}

/// <summary>Outcome of a completed tenant-provisioning pipeline run.</summary>
/// <param name="Tenant">The newly provisioned (now Active) tenant's read model.</param>
/// <param name="AdminUserId">Identity of the tenant's newly created administrative user.</param>
/// <param name="ApiKey">
/// The tenant's newly generated primary API key, in plaintext -- shown exactly once. Callers must
/// deliver it to the tenant immediately; it is never retrievable again (see
/// <see cref="Abstractions.ITenantApiKeyGenerator"/>).
/// </param>
public sealed record TenantProvisioningResultDto(TenantDto Tenant, Guid AdminUserId, string ApiKey);

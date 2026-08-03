// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Operation context interface carrying the full correlation lineage envelope.
//   Every layer MUST propagate this envelope (§2.17-2.18 Correlation & Causality Lineage Rule).
//   CorrelationId is generated once at the API boundary and remains constant for the entire operation.
//   CausationId identifies the immediate parent command/event and is NEVER regenerated for child operations.
// SchemaVersion: 1
// Additive-only evolution: new members may be added; no members may be removed or renamed.

using Shared.Authorization.Abstractions.Roles;

namespace PlantSafety.Application.Abstractions.Context;

/// <summary>
/// Carries the full canonical correlation lineage envelope required by §2.17-2.18.
/// This context is injected into every handler, propagated through all layers,
/// and included in every structured log entry and domain event.
/// </summary>
/// <remarks>
/// Causation flow: Root commands generate new <c>CausationId</c> → child commands inherit
/// the parent's <c>CausationId</c> → domain events inherit → audit entries inherit.
/// The MediatR pipeline injects this context if missing; it must never be null at handler execution time.
/// </remarks>
public interface IOperationContext
{
    /// <summary>Gets the correlation identifier that groups all operations in a single business transaction.
    /// Generated once at the API boundary; constant for the entire operation tree.</summary>
    Guid CorrelationId { get; }

    /// <summary>Gets the causation identifier that identifies the immediate parent command or event.
    /// Null only for root operations. MUST be inherited, never regenerated for child operations.</summary>
    Guid? CausationId { get; }

    /// <summary>Gets the inbound API request identifier.</summary>
    Guid RequestId { get; }

    /// <summary>Gets the distributed trace identifier for OpenTelemetry propagation.</summary>
    string TraceId { get; }

    /// <summary>Gets the current span identifier for OpenTelemetry.</summary>
    string SpanId { get; }

    /// <summary>Gets the upstream workflow step identifier for long-running saga tracking.</summary>
    string? ParentOperationId { get; }

    /// <summary>Gets the tenant identifier for multi-tenancy context.</summary>
    Guid TenantId { get; }

    /// <summary>Gets the site identifier if the operation is site-scoped.</summary>
    Guid? SiteId { get; }

    /// <summary>Gets the UTC timestamp when the root operation began.</summary>
    DateTime OperationStartedAtUtc { get; }

    /// <summary>
    /// Gets the formalized role context for the current session (Phase 6 §2.4
    /// "Session Context Hardening"). Carries the resolved <see cref="EnterpriseRole"/>
    /// tier alongside the raw role-claim strings, so downstream handlers/behaviors do not
    /// each re-derive the tier from <c>ICurrentUserService.Roles</c> independently.
    /// </summary>
    /// <remarks>
    /// Additive member (SchemaVersion 1 → 2). Added 2026-08-02. Default implementations
    /// (test doubles via NSubstitute, etc.) resolve this to <see cref="RoleContext.Empty"/>
    /// automatically; no existing consumer of <see cref="IOperationContext"/> is broken by
    /// this addition, since it is a new property, not a change to any existing member.
    /// </remarks>
    RoleContext RoleContext { get; }

    /// <summary>
    /// Gets the formalized compliance context for the current session (Phase 6 §2.4
    /// "Session Context Hardening"). Captures whether the current operation is under a
    /// specific regulatory scope (e.g. OSHA, ISO 45001, a customer-specific compliance
    /// program), for compliance-aware behaviors/audit trails to consult without each
    /// re-deriving it from tenant configuration.
    /// </summary>
    /// <remarks>
    /// Additive member (SchemaVersion 1 → 2). Added 2026-08-02.
    /// </remarks>
    ComplianceContext ComplianceContext { get; }
}

/// <summary>
/// Formalized role context carried by <see cref="IOperationContext"/>. Additive extension
/// (Phase 6 §2.4) that resolves the current session's highest-ranked
/// <see cref="EnterpriseRole"/> alongside the full raw role-claim set, without
/// replacing <c>ICurrentUserService.Roles</c> (the existing source of truth for raw
/// role claims).
/// </summary>
/// <param name="Roles">The raw role-claim strings held by the current principal.</param>
/// <param name="HighestRole">
/// The highest-ranked <see cref="EnterpriseRole"/> resolved from <paramref name="Roles"/>,
/// or <see langword="null"/> when none of the raw claims match a canonical enterprise role
/// (e.g. an unauthenticated request, or a role outside the enterprise tier model).
/// </param>
public sealed record RoleContext(
    IReadOnlyList<string> Roles,
    EnterpriseRole? HighestRole)
{
    /// <summary>An empty role context — no roles resolved (e.g. unauthenticated request or non-HTTP context).</summary>
    public static readonly RoleContext Empty = new(Array.Empty<string>(), null);
}

/// <summary>
/// Formalized compliance context carried by <see cref="IOperationContext"/> (Phase 6 §2.4
/// "Session Context Hardening"). Captures whether the current session/operation is under a
/// specific regulatory scope.
/// </summary>
/// <param name="IsRegulatedSession">
/// <see langword="true"/> when this operation is subject to a specific regulatory program
/// (e.g. OSHA-regulated site, ISO 45001-certified tenant); <see langword="false"/> for
/// standard, non-regulated operations.
/// </param>
/// <param name="RegulatoryScope">
/// The regulatory program/scope identifier (e.g. <c>"OSHA"</c>, <c>"ISO45001"</c>), or
/// <see langword="null"/> when <paramref name="IsRegulatedSession"/> is <see langword="false"/>.
/// </param>
public sealed record ComplianceContext(
    bool IsRegulatedSession,
    string? RegulatoryScope)
{
    /// <summary>The default, non-regulated compliance context.</summary>
    public static readonly ComplianceContext None = new(false, null);
}

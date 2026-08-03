// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Canonical SharedKernel contract for cross-module read-model composition
//   (Handbook Part IV §4.2.5 "Integration Happens Only After Modules Are Stable" and Part IX
//   §9.3.2 "Modules May Only Communicate via Public Contracts" -- read models are an explicitly
//   allowed integration mechanism). A composing module (e.g. a "rollup report" submodule that
//   aggregates data owned by several maximalist sibling submodules) MUST NOT dispatch another
//   submodule's Command/Query objects directly through ISender/MediatR -- that is Application-to-
//   Application cross-module coupling and is prohibited. Instead, each contributing submodule
//   implements and registers exactly one IReportSectionProvider&lt;TRequest, TSection&gt; per
//   read-model section it exposes for composition, and the composing module resolves the provider
//   through ordinary DI (constructor injection) rather than through a transferable Query object.
// SchemaVersion: 1
// Additive-only evolution: new members may be added to this file's types; no existing member may
//   be removed, renamed, or have its semantics changed.

using PlantSafety.SharedKernel.Primitives.Results;

namespace PlantSafety.SharedKernel.Contracts.ReadModelComposition;

/// <summary>
/// Canonical contract implemented by a contributing submodule's Application layer to expose one
/// read-model section for composition by another (sibling) submodule's aggregate report or
/// response, without requiring the composing submodule to reference or dispatch the contributing
/// submodule's Commands, Queries, repositories, aggregates, domain services, or validators.
/// </summary>
/// <typeparam name="TRequest">
/// The deterministic, DTO-shaped request/context envelope required to produce the section. Owned
/// by the contributing submodule (it is part of that submodule's public read-model contract) and
/// MUST contain no domain logic -- only the data required to scope the query (e.g. TenantId,
/// EntityId, date range, paging).
/// </typeparam>
/// <typeparam name="TSection">
/// The read-model DTO produced for the section. MUST be a public, immutable DTO already exposed by
/// the contributing submodule's Application layer (e.g. the same DTO its own Query returns) --
/// never a domain entity, aggregate, or infrastructure type.
/// </typeparam>
/// <remarks>
/// Implementations live inside the contributing submodule (e.g.
/// <c>Auditing.AuditTimeline.Contracts.AuditTimelineSectionProvider</c>) and are registered by that
/// submodule's own Application-layer DI registration extension method. Composing submodules (e.g.
/// <c>Auditing.AuditReport</c>, <c>Operations.SystemHealth</c>) consume this interface via
/// constructor injection -- resolved by DI, never dispatched via <c>ISender.Send</c>. Intra-module
/// use of MediatR (a submodule invoking its own Command/Query) remains fully permitted; only
/// cross-submodule composition MUST route through this contract.
/// </remarks>
public interface IReportSectionProvider<in TRequest, TSection>
{
    /// <summary>
    /// Stable, human-readable identifier for this section, used for logging, diagnostics, and
    /// resilient "section unavailable" reporting by the composing module. MUST be deterministic
    /// and MUST NOT change once published (additive-only evolution).
    /// </summary>
    string SectionName { get; }

    /// <summary>
    /// Produces the read-model section for the supplied request context.
    /// </summary>
    /// <param name="request">The deterministic request/context envelope scoping the section.</param>
    /// <param name="cancellationToken">Propagated cancellation token.</param>
    /// <returns>
    /// A <see cref="Result{TSection}"/> containing the section DTO on success, or a canonical
    /// <see cref="Primitives.Errors.Error"/> describing the failure. This method MUST NOT throw for
    /// expected failure conditions; it MUST return a failed <see cref="Result{TSection}"/> instead.
    /// </returns>
    Task<Result<TSection>> GetSectionAsync(TRequest request, CancellationToken cancellationToken);
}

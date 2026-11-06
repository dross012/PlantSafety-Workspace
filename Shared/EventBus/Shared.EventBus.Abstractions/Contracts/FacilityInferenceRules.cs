// ============================================================================
//  Project:        Shared.EventBus.Abstractions
//  File:           FacilityInferenceRules.cs
//  Created:        2026-08-08
//  Description:    Phase B.4 migration scaffolding (TEMPORARY). Shared.
//                  EventBus cannot depend on a specific facility-identity
//                  bounded context (see EventEnvelopeV2.cs's header), so
//                  facility resolution is expressed as a resolver
//                  INTERFACE here; the actual lookup (e.g. against
//                  Shared.FacilityDomain.Abstractions.Repositories.
//                  IFacilityService, once a real tenant-to-facility
//                  binding exists) is implemented by the consuming
//                  application (PlantSafety.Infrastructure), not by
//                  Shared.EventBus itself.
//
//                  NullFacilityInferenceResolver is the honest default:
//                  no real tenant-to-facility binding is populated
//                  anywhere yet (Shared.FacilityDomain's store is brand
//                  new and empty), so resolving "unknown" for every
//                  tenant is the correct behavior today, not a stand-in
//                  for a smarter algorithm that hasn't been written.
//                  Replace the registered implementation once a real
//                  binding source exists; delete this file once every
//                  event reliably carries a real FacilityId some other
//                  way.
// ============================================================================
namespace Shared.EventBus.Abstractions.Contracts;

/// <summary>Resolves the facility (id/industry/location) associated with a tenant, if known.</summary>
public interface IFacilityInferenceResolver
{
    /// <summary>Attempts to resolve the facility bound to <paramref name="tenantId"/>. All three values are null together if no binding is known.</summary>
    Task<(Guid? FacilityId, string? IndustryName, string? Location)> ResolveAsync(string tenantId, CancellationToken cancellationToken = default);
}

/// <summary>Default <see cref="IFacilityInferenceResolver"/>: always resolves "unknown" (no binding is populated anywhere yet).</summary>
public sealed class NullFacilityInferenceResolver : IFacilityInferenceResolver
{
    /// <inheritdoc />
    public Task<(Guid? FacilityId, string? IndustryName, string? Location)> ResolveAsync(string tenantId, CancellationToken cancellationToken = default) =>
        Task.FromResult<(Guid?, string?, string?)>((null, null, null));
}

/// <summary>Enriches an <see cref="EventEnvelopeV2"/> with the facility resolved for its TenantId.</summary>
public static class FacilityInferenceRules
{
    /// <summary>Resolves and applies facility enrichment to <paramref name="envelope"/>, returning the (possibly unchanged) result.</summary>
    public static async Task<EventEnvelopeV2> EnrichWithFacilityAsync(EventEnvelopeV2 envelope, IFacilityInferenceResolver resolver, CancellationToken cancellationToken = default)
    {
        (Guid? facilityId, string? industryName, string? location) = await resolver.ResolveAsync(envelope.TenantId, cancellationToken);
        return envelope with { FacilityId = facilityId, IndustryName = industryName, Location = location };
    }
}

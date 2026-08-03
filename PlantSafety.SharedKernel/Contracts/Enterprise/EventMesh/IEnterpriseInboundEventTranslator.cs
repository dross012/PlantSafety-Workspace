// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel.Contracts.Enterprise.EventMesh
//  Layer:          SharedKernel
//  File:           IEnterpriseInboundEventTranslator.cs
//  Created:        2026-08-03
//  Author:         PlantSafety Platform Team (Phase 6 §3.2 Cross-System Event Propagation, Eventing & Messaging Agent)
//  SchemaVersion:  2026.1
//  Description:    Contract implemented once per inbound (external-system -> platform) event type
//                  the Enterprise Event Mesh understands (e.g. an ERP work-order-sync event).
//                  Implementations live in PlantSafety.Application (so they may dispatch the target
//                  module's real, public Application-layer Commands via ISender per Handbook Part IV
//                  §4.2.2/Part IX -- never by reaching into another module's Domain internals) and
//                  are resolved by the inbound adapter (controller or queue consumer) via ordinary DI
//                  as an <c>IEnumerable&lt;IEnterpriseInboundEventTranslator&gt;</c>, matching the
//                  <see cref="ReadModelComposition.IReportSectionProvider{TRequest,TSection}"/>
//                  precedent for "resolve via DI, not via a transferable cross-module Query/Command
//                  object". Additive-only evolution.
// ============================================================================
using PlantSafety.SharedKernel.Primitives.Results;

namespace PlantSafety.SharedKernel.Contracts.Enterprise.EventMesh;

/// <summary>
/// Translates one inbound <see cref="EnterpriseEventEnvelope"/> event type from an external system
/// into the platform's real internal commands/domain events.
/// </summary>
public interface IEnterpriseInboundEventTranslator
{
    /// <summary>Returns <see langword="true"/> if this translator handles <paramref name="envelope"/>'s EventType.</summary>
    bool CanHandle(EnterpriseEventEnvelope envelope);

    /// <summary>Translates and dispatches <paramref name="envelope"/>, returning the outcome of the resulting internal operation.</summary>
    Task<Result<bool>> TranslateAndDispatchAsync(EnterpriseEventEnvelope envelope, CancellationToken cancellationToken);
}

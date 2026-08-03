// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: AuditTimeline's implementation of the SharedKernel
//   IReportSectionProvider&lt;AuditTimelineSectionRequest, AuditTimelineDto&gt; contract. Dispatches
//   GetAuditTimelineForEntityQuery via ISender -- which remains fully permitted because the
//   dispatch stays entirely inside the AuditTimeline submodule -- and adapts the Application-layer
//   Result&lt;T&gt;/ApplicationError into the SharedKernel's canonical Result&lt;T&gt;/Error so that
//   sibling submodules (e.g. AuditReport) can compose this section without referencing
//   AuditTimeline's Queries namespace at all. Registered by
//   AuditTimelineApplicationServiceRegistration.
// SchemaVersion: 1

using MediatR;
using PlantSafety.SharedKernel.Contracts.ReadModelComposition;
using PlantSafety.SharedKernel.Primitives.Errors;
using SharedResult = PlantSafety.SharedKernel.Primitives.Results.Result<PlantSafety.Application.Auditing.AuditTimeline.Models.AuditTimelineDto>;
using PlantSafety.Application.Auditing.AuditTimeline.Models;
using PlantSafety.Application.Auditing.AuditTimeline.Queries;

namespace PlantSafety.Application.Auditing.AuditTimeline.Contracts;

/// <summary>
/// AuditTimeline's <see cref="IReportSectionProvider{TRequest, TSection}"/> implementation,
/// exposing the composed AuditTimeline read-model as a public cross-module contract.
/// </summary>
public sealed class AuditTimelineSectionProvider : IReportSectionProvider<AuditTimelineSectionRequest, AuditTimelineDto>
{
    private readonly ISender _sender;

    /// <summary>Initialises the provider with the Application layer's <see cref="ISender"/>.</summary>
    public AuditTimelineSectionProvider(ISender sender)
    {
        _sender = sender;
    }

    /// <inheritdoc/>
    public string SectionName => "AuditTimeline";

    /// <inheritdoc/>
    public async Task<SharedResult> GetSectionAsync(AuditTimelineSectionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetAuditTimelineForEntityQuery(request.TenantId, request.EntityType, request.EntityId),
            cancellationToken);

        return result.IsSuccess
            ? SharedResult.Success(result.Value)
            : SharedResult.Failure(ErrorFactory.Create(result.Error.ErrorCode, ErrorType.Failure, result.Error.Message));
    }
}

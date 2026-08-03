// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Operations.AuditViewer's implementation of the SharedKernel
//   IReportSectionProvider<RecentAuditActivitySectionRequest, int> contract. Dispatches
//   SearchAuditLogQuery (PageSize=1 -- only TotalCount is needed) via ISender -- fully permitted
//   because the dispatch stays entirely inside the AuditViewer submodule -- and adapts the
//   Application-layer Result<T>/ApplicationError into the SharedKernel's canonical
//   Result<T>/Error so that Operations.SystemHealth can compose this section without referencing
//   Operations.AuditViewer.Queries at all.
// SchemaVersion: 1

using MediatR;
using PlantSafety.SharedKernel.Contracts.ReadModelComposition;
using PlantSafety.SharedKernel.Primitives.Errors;
using Shared.AuditLogging.Abstractions.Models;
using PlantSafety.Application.Operations.AuditViewer.Queries;
using SharedResult = PlantSafety.SharedKernel.Primitives.Results.Result<int>;

namespace PlantSafety.Application.Operations.AuditViewer.Contracts;

/// <summary>
/// Operations.AuditViewer's <see cref="IReportSectionProvider{TRequest, TSection}"/>
/// implementation, exposing a recent-activity row count as a public cross-module read-model
/// contract.
/// </summary>
public sealed class RecentAuditActivitySectionProvider : IReportSectionProvider<RecentAuditActivitySectionRequest, int>
{
    private readonly ISender _sender;

    /// <summary>Initialises the provider with the Application layer's <see cref="ISender"/>.</summary>
    public RecentAuditActivitySectionProvider(ISender sender)
    {
        _sender = sender;
    }

    /// <inheritdoc/>
    public string SectionName => "RecentAuditActivityCount";

    /// <inheritdoc/>
    public async Task<SharedResult> GetSectionAsync(RecentAuditActivitySectionRequest request, CancellationToken cancellationToken)
    {
        var criteria = new AuditLogSearchCriteria(FromUtc: request.SinceUtc, PageSize: 1);
        var result = await _sender.Send(new SearchAuditLogQuery(criteria), cancellationToken);

        return result.IsSuccess
            ? SharedResult.Success(result.Value.TotalCount)
            : SharedResult.Failure(ErrorFactory.Create(result.Error.ErrorCode, ErrorType.Failure, result.Error.Message));
    }
}

// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Operations.Monitoring's implementation of the SharedKernel
//   IReportSectionProvider<QueryPerformanceSectionRequest, IReadOnlyList<QueryPerformanceItemDto>>
//   contract. Dispatches GetQueryPerformanceQuery via ISender -- fully permitted because the
//   dispatch stays entirely inside the Monitoring submodule -- and adapts the Application-layer
//   Result<T>/ApplicationError into the SharedKernel's canonical Result<T>/Error so that
//   Operations.SystemHealth can compose this section without referencing
//   Operations.Monitoring.Queries at all.
// SchemaVersion: 1

using MediatR;
using PlantSafety.SharedKernel.Contracts.ReadModelComposition;
using PlantSafety.SharedKernel.Primitives.Errors;
using PlantSafety.Application.Operations.Monitoring.Models;
using PlantSafety.Application.Operations.Monitoring.Queries;
using SharedResult = PlantSafety.SharedKernel.Primitives.Results.Result<System.Collections.Generic.IReadOnlyList<PlantSafety.Application.Operations.Monitoring.Models.QueryPerformanceItemDto>>;

namespace PlantSafety.Application.Operations.Monitoring.Contracts;

/// <summary>
/// Operations.Monitoring's <see cref="IReportSectionProvider{TRequest, TSection}"/> implementation,
/// exposing top queries by average CPU time as a public cross-module read-model contract.
/// </summary>
public sealed class QueryPerformanceSectionProvider : IReportSectionProvider<QueryPerformanceSectionRequest, IReadOnlyList<QueryPerformanceItemDto>>
{
    private readonly ISender _sender;

    /// <summary>Initialises the provider with the Application layer's <see cref="ISender"/>.</summary>
    public QueryPerformanceSectionProvider(ISender sender)
    {
        _sender = sender;
    }

    /// <inheritdoc/>
    public string SectionName => "QueryPerformance";

    /// <inheritdoc/>
    public async Task<SharedResult> GetSectionAsync(QueryPerformanceSectionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetQueryPerformanceQuery(request.DatabaseKey, request.Top, request.LookbackHours),
            cancellationToken);

        return result.IsSuccess
            ? SharedResult.Success(result.Value)
            : SharedResult.Failure(ErrorFactory.Create(result.Error.ErrorCode, ErrorType.Failure, result.Error.Message));
    }
}

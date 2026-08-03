// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Operations.Archival's implementation of the SharedKernel
//   IReportSectionProvider<PurgeStatusSectionRequest, IReadOnlyList<ArchivalJobDto>> contract.
//   Dispatches GetPurgeStatusQuery via ISender -- fully permitted because the dispatch stays
//   entirely inside the Archival submodule -- and adapts the Application-layer
//   Result<T>/ApplicationError into the SharedKernel's canonical Result<T>/Error so that
//   Operations.SystemHealth can compose this section without referencing
//   Operations.Archival.Queries at all.
// SchemaVersion: 1

using MediatR;
using PlantSafety.SharedKernel.Contracts.ReadModelComposition;
using PlantSafety.SharedKernel.Primitives.Errors;
using PlantSafety.Application.Operations.Archival.Models;
using PlantSafety.Application.Operations.Archival.Queries;
using SharedResult = PlantSafety.SharedKernel.Primitives.Results.Result<System.Collections.Generic.IReadOnlyList<PlantSafety.Application.Operations.Archival.Models.ArchivalJobDto>>;

namespace PlantSafety.Application.Operations.Archival.Contracts;

/// <summary>
/// Operations.Archival's <see cref="IReportSectionProvider{TRequest, TSection}"/> implementation,
/// exposing recent purge run history as a public cross-module read-model contract.
/// </summary>
public sealed class PurgeStatusSectionProvider : IReportSectionProvider<PurgeStatusSectionRequest, IReadOnlyList<ArchivalJobDto>>
{
    private readonly ISender _sender;

    /// <summary>Initialises the provider with the Application layer's <see cref="ISender"/>.</summary>
    public PurgeStatusSectionProvider(ISender sender)
    {
        _sender = sender;
    }

    /// <inheritdoc/>
    public string SectionName => "PurgeStatus";

    /// <inheritdoc/>
    public async Task<SharedResult> GetSectionAsync(PurgeStatusSectionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetPurgeStatusQuery(request.Zone, request.Top), cancellationToken);

        return result.IsSuccess
            ? SharedResult.Success(result.Value)
            : SharedResult.Failure(ErrorFactory.Create(result.Error.ErrorCode, ErrorType.Failure, result.Error.Message));
    }
}

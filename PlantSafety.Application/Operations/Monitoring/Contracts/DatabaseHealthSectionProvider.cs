// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Operations.Monitoring's implementation of the SharedKernel
//   IReportSectionProvider<NoSectionRequest, DatabaseHealthReportDto> contract. Dispatches
//   GetDatabaseHealthReportQuery via ISender -- fully permitted because the dispatch stays
//   entirely inside the Monitoring submodule -- and adapts the Application-layer
//   Result<T>/ApplicationError into the SharedKernel's canonical Result<T>/Error so that
//   Operations.SystemHealth can compose this section without referencing
//   Operations.Monitoring.Queries at all.
// SchemaVersion: 1

using MediatR;
using PlantSafety.SharedKernel.Contracts.ReadModelComposition;
using PlantSafety.SharedKernel.Primitives.Errors;
using PlantSafety.Application.Operations.Monitoring.Models;
using PlantSafety.Application.Operations.Monitoring.Queries;
using SharedResult = PlantSafety.SharedKernel.Primitives.Results.Result<PlantSafety.Application.Operations.Monitoring.Models.DatabaseHealthReportDto>;

namespace PlantSafety.Application.Operations.Monitoring.Contracts;

/// <summary>
/// Operations.Monitoring's <see cref="IReportSectionProvider{TRequest, TSection}"/> implementation,
/// exposing the aggregated /health/ready readiness report as a public cross-module read-model
/// contract.
/// </summary>
public sealed class DatabaseHealthSectionProvider : IReportSectionProvider<NoSectionRequest, DatabaseHealthReportDto>
{
    private readonly ISender _sender;

    /// <summary>Initialises the provider with the Application layer's <see cref="ISender"/>.</summary>
    public DatabaseHealthSectionProvider(ISender sender)
    {
        _sender = sender;
    }

    /// <inheritdoc/>
    public string SectionName => "DatabaseHealth";

    /// <inheritdoc/>
    public async Task<SharedResult> GetSectionAsync(NoSectionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetDatabaseHealthReportQuery(), cancellationToken);

        return result.IsSuccess
            ? SharedResult.Success(result.Value)
            : SharedResult.Failure(ErrorFactory.Create(result.Error.ErrorCode, ErrorType.Failure, result.Error.Message));
    }
}

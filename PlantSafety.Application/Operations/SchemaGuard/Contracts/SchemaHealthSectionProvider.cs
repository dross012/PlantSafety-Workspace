// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Operations.SchemaGuard's implementation of the SharedKernel
//   IReportSectionProvider<NoSectionRequest, IReadOnlyList<SchemaDatabaseHealthDto>> contract.
//   Dispatches GetSchemaHealthQuery via ISender -- fully permitted because the dispatch stays
//   entirely inside the SchemaGuard submodule -- and adapts the Application-layer
//   Result<T>/ApplicationError into the SharedKernel's canonical Result<T>/Error so that
//   Operations.SystemHealth can compose this section without referencing
//   Operations.SchemaGuard.Queries at all.
// SchemaVersion: 1

using MediatR;
using PlantSafety.SharedKernel.Contracts.ReadModelComposition;
using PlantSafety.SharedKernel.Primitives.Errors;
using PlantSafety.Application.Operations.SchemaGuard.Models;
using PlantSafety.Application.Operations.SchemaGuard.Queries;
using SharedResult = PlantSafety.SharedKernel.Primitives.Results.Result<System.Collections.Generic.IReadOnlyList<PlantSafety.Application.Operations.SchemaGuard.Models.SchemaDatabaseHealthDto>>;

namespace PlantSafety.Application.Operations.SchemaGuard.Contracts;

/// <summary>
/// Operations.SchemaGuard's <see cref="IReportSectionProvider{TRequest, TSection}"/>
/// implementation, exposing per-database schema-drift status as a public cross-module read-model
/// contract.
/// </summary>
public sealed class SchemaHealthSectionProvider : IReportSectionProvider<NoSectionRequest, IReadOnlyList<SchemaDatabaseHealthDto>>
{
    private readonly ISender _sender;

    /// <summary>Initialises the provider with the Application layer's <see cref="ISender"/>.</summary>
    public SchemaHealthSectionProvider(ISender sender)
    {
        _sender = sender;
    }

    /// <inheritdoc/>
    public string SectionName => "SchemaHealth";

    /// <inheritdoc/>
    public async Task<SharedResult> GetSectionAsync(NoSectionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetSchemaHealthQuery(), cancellationToken);

        return result.IsSuccess
            ? SharedResult.Success(result.Value)
            : SharedResult.Failure(ErrorFactory.Create(result.Error.ErrorCode, ErrorType.Failure, result.Error.Message));
    }
}

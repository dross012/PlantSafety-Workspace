// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Operations.Backup's implementation of the SharedKernel
//   IReportSectionProvider<NoSectionRequest, IReadOnlyList<BackupHealthDto>> contract. Dispatches
//   GetBackupHealthQuery via ISender -- fully permitted because the dispatch stays entirely inside
//   the Backup submodule -- and adapts the Application-layer Result<T>/ApplicationError into the
//   SharedKernel's canonical Result<T>/Error so that Operations.SystemHealth can compose this
//   section without referencing Operations.Backup.Queries at all.
// SchemaVersion: 1

using MediatR;
using PlantSafety.SharedKernel.Contracts.ReadModelComposition;
using PlantSafety.SharedKernel.Primitives.Errors;
using PlantSafety.Application.Operations.Backup.Models;
using PlantSafety.Application.Operations.Backup.Queries;
using SharedResult = PlantSafety.SharedKernel.Primitives.Results.Result<System.Collections.Generic.IReadOnlyList<PlantSafety.Application.Operations.Backup.Models.BackupHealthDto>>;

namespace PlantSafety.Application.Operations.Backup.Contracts;

/// <summary>
/// Operations.Backup's <see cref="IReportSectionProvider{TRequest, TSection}"/> implementation,
/// exposing derived backup-health status as a public cross-module read-model contract.
/// </summary>
public sealed class BackupHealthSectionProvider : IReportSectionProvider<NoSectionRequest, IReadOnlyList<BackupHealthDto>>
{
    private readonly ISender _sender;

    /// <summary>Initialises the provider with the Application layer's <see cref="ISender"/>.</summary>
    public BackupHealthSectionProvider(ISender sender)
    {
        _sender = sender;
    }

    /// <inheritdoc/>
    public string SectionName => "BackupHealth";

    /// <inheritdoc/>
    public async Task<SharedResult> GetSectionAsync(NoSectionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetBackupHealthQuery(), cancellationToken);

        return result.IsSuccess
            ? SharedResult.Success(result.Value)
            : SharedResult.Failure(ErrorFactory.Create(result.Error.ErrorCode, ErrorType.Failure, result.Error.Message));
    }
}

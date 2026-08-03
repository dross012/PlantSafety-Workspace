// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-03
// FILE DESCRIPTION: Operations.Configuration's implementation of the SharedKernel
//   IReportSectionProvider<NoSectionRequest, IReadOnlyList<FeatureFlagDto>> contract. Dispatches
//   GetAllFeatureFlagsQuery via ISender -- fully permitted because the dispatch stays entirely
//   inside the Configuration submodule -- and adapts the Application-layer
//   Result<T>/ApplicationError into the SharedKernel's canonical Result<T>/Error so that
//   Operations.SystemHealth can compose this section without referencing
//   Operations.Configuration.Queries at all.
// SchemaVersion: 1

using MediatR;
using PlantSafety.SharedKernel.Contracts.ReadModelComposition;
using PlantSafety.SharedKernel.Primitives.Errors;
using PlantSafety.Application.Operations.Configuration.Models;
using PlantSafety.Application.Operations.Configuration.Queries;
using SharedResult = PlantSafety.SharedKernel.Primitives.Results.Result<System.Collections.Generic.IReadOnlyList<PlantSafety.Application.Operations.Configuration.Models.FeatureFlagDto>>;

namespace PlantSafety.Application.Operations.Configuration.Contracts;

/// <summary>
/// Operations.Configuration's <see cref="IReportSectionProvider{TRequest, TSection}"/>
/// implementation, exposing every known feature flag as a public cross-module read-model contract.
/// </summary>
public sealed class FeatureFlagsSectionProvider : IReportSectionProvider<NoSectionRequest, IReadOnlyList<FeatureFlagDto>>
{
    private readonly ISender _sender;

    /// <summary>Initialises the provider with the Application layer's <see cref="ISender"/>.</summary>
    public FeatureFlagsSectionProvider(ISender sender)
    {
        _sender = sender;
    }

    /// <inheritdoc/>
    public string SectionName => "FeatureFlags";

    /// <inheritdoc/>
    public async Task<SharedResult> GetSectionAsync(NoSectionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetAllFeatureFlagsQuery(), cancellationToken);

        return result.IsSuccess
            ? SharedResult.Success(result.Value)
            : SharedResult.Failure(ErrorFactory.Create(result.Error.ErrorCode, ErrorType.Failure, result.Error.Message));
    }
}

// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for GetFmcsaStandardQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.FMCSAStandards.Models;
using PlantSafety.Application.Compliance.FMCSAStandards.Repositories;

namespace PlantSafety.Application.Compliance.FMCSAStandards.Queries;

/// <summary>Handles <see cref="GetFmcsaStandardQuery"/>.</summary>
public sealed class GetFmcsaStandardQueryHandler(
    IFmcsaStandardsRepository repository,
    ILogger<GetFmcsaStandardQueryHandler> logger)
    : IRequestHandler<GetFmcsaStandardQuery, Result<FmcsaStandardDto>>
{
    /// <inheritdoc/>
    public async Task<Result<FmcsaStandardDto>> Handle(GetFmcsaStandardQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var dto = await repository.GetStandardBySectionAsync(request.PartNumber, request.SectionNumber, cancellationToken);

            return dto is null
                ? Result<FmcsaStandardDto>.Failure(new ApplicationError(
                    "Compliance.FMCSAStandards.NotFound",
                    "The requested FMCSA standard was not found.",
                    Guid.NewGuid()))
                : Result<FmcsaStandardDto>.Success(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve FMCSA standard Part {PartNumber} Section {SectionNumber}", request.PartNumber, request.SectionNumber);

            return Result<FmcsaStandardDto>.Failure(new ApplicationError(
                "Compliance.FMCSAStandards.RetrievalFailed",
                "Unable to retrieve the FMCSA standard at this time.",
                Guid.NewGuid()));
        }
    }
}

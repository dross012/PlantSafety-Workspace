// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Handler for GetNfpaHazardProfileQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.NFPAStandards.Models;
using PlantSafety.Application.Compliance.NFPAStandards.Repositories;

namespace PlantSafety.Application.Compliance.NFPAStandards.Queries;

/// <summary>Handles <see cref="GetNfpaHazardProfileQuery"/>.</summary>
public sealed class GetNfpaHazardProfileQueryHandler(
    INfpaReferenceRepository repository,
    ILogger<GetNfpaHazardProfileQueryHandler> logger)
    : IRequestHandler<GetNfpaHazardProfileQuery, Result<NfpaHazardProfileDto>>
{
    /// <inheritdoc/>
    public async Task<Result<NfpaHazardProfileDto>> Handle(GetNfpaHazardProfileQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var dto = await repository.GetHazardProfileAsync(request.CodeNumber, request.SectionNumber, cancellationToken);

            return dto is null
                ? Result<NfpaHazardProfileDto>.Failure(new ApplicationError(
                    "Compliance.NFPAStandards.NotFound",
                    "The requested NFPA hazard profile was not found.",
                    Guid.NewGuid()))
                : Result<NfpaHazardProfileDto>.Success(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve NFPA hazard profile Code {CodeNumber} Section {SectionNumber}", request.CodeNumber, request.SectionNumber);

            return Result<NfpaHazardProfileDto>.Failure(new ApplicationError(
                "Compliance.NFPAStandards.RetrievalFailed",
                "Unable to retrieve the NFPA hazard profile at this time.",
                Guid.NewGuid()));
        }
    }
}

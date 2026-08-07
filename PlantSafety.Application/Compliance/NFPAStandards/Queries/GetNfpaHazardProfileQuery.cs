// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-07
// FILE DESCRIPTION: Query to retrieve a single NFPA hazard profile by (CodeNumber, SectionNumber).
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.NFPAStandards.Models;

namespace PlantSafety.Application.Compliance.NFPAStandards.Queries;

/// <summary>Retrieves a single NFPA hazard profile by (CodeNumber, SectionNumber).</summary>
/// <param name="CodeNumber">NFPA code number (e.g. "70").</param>
/// <param name="SectionNumber">Section citation within the code (e.g. "110.26").</param>
public sealed record GetNfpaHazardProfileQuery(
    string CodeNumber,
    string SectionNumber) : IRequest<Result<NfpaHazardProfileDto>>;

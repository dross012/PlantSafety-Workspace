// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Query to retrieve the full disposal-certificate history for a WasteStream.
// SchemaVersion: 1

using MediatR;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.HazardousWasteManagement.DisposalCertificates.Queries;

/// <summary>Retrieves the disposal certificate history for the given waste stream.</summary>
/// <param name="TenantId">Tenant identifier resolved from JWT claims.</param>
/// <param name="WasteStreamId">Identity of the waste stream to retrieve disposal history for.</param>
public sealed record GetDisposalHistoryByStreamQuery(
    Guid TenantId,
    Guid WasteStreamId) : IRequest<Result<GetDisposalHistoryByStreamResult>>;

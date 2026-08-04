// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for GetDisposalHistoryByStreamQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.HazardousWasteManagement.DisposalCertificates.Queries;
using PlantSafety.Application.HazardousWasteManagement.DisposalCertificates.Repositories;

namespace PlantSafety.Application.HazardousWasteManagement.DisposalCertificates.Handlers;

/// <summary>Handles <see cref="GetDisposalHistoryByStreamQuery"/>.</summary>
public sealed class GetDisposalHistoryByStreamQueryHandler : IRequestHandler<GetDisposalHistoryByStreamQuery, Result<GetDisposalHistoryByStreamResult>>
{
    private readonly IDisposalCertificateRepository _repository;
    private readonly ILogger<GetDisposalHistoryByStreamQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetDisposalHistoryByStreamQueryHandler(IDisposalCertificateRepository repository, ILogger<GetDisposalHistoryByStreamQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<GetDisposalHistoryByStreamResult>> Handle(GetDisposalHistoryByStreamQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetDisposalHistoryByStreamAsync(request.TenantId, request.WasteStreamId, cancellationToken);
            return Result<GetDisposalHistoryByStreamResult>.Success(new GetDisposalHistoryByStreamResult(items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve disposal history for WasteStream {WasteStreamId}", request.WasteStreamId);

            return Result<GetDisposalHistoryByStreamResult>.Failure(new ApplicationError(
                "HazardousWasteManagement.DisposalCertificate.RetrievalFailed",
                "Unable to retrieve disposal history at this time.",
                Guid.NewGuid()));
        }
    }
}

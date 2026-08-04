// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for GetWasteStreamByIdQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.HazardousWasteManagement.WasteStreams.Queries;
using PlantSafety.Application.HazardousWasteManagement.WasteStreams.Repositories;

namespace PlantSafety.Application.HazardousWasteManagement.WasteStreams.Handlers;

/// <summary>Handles <see cref="GetWasteStreamByIdQuery"/>.</summary>
public sealed class GetWasteStreamByIdQueryHandler : IRequestHandler<GetWasteStreamByIdQuery, Result<GetWasteStreamByIdResult>>
{
    private readonly IWasteStreamRepository _repository;
    private readonly ILogger<GetWasteStreamByIdQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetWasteStreamByIdQueryHandler(IWasteStreamRepository repository, ILogger<GetWasteStreamByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<GetWasteStreamByIdResult>> Handle(GetWasteStreamByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _repository.GetWasteStreamByIdAsync(request.TenantId, request.WasteStreamId, cancellationToken);
            return Result<GetWasteStreamByIdResult>.Success(new GetWasteStreamByIdResult(item));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve WasteStream {WasteStreamId}", request.WasteStreamId);

            return Result<GetWasteStreamByIdResult>.Failure(new ApplicationError(
                "HazardousWasteManagement.WasteStream.RetrievalFailed",
                "Unable to retrieve the waste stream at this time.",
                Guid.NewGuid()));
        }
    }
}

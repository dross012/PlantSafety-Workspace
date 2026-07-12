// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Handler for GetIoTQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Integrations.IoT.Queries;
using PlantSafety.Application.Integrations.IoT.Repositories;

namespace PlantSafety.Application.Integrations.IoT.Handlers;

/// <summary>Handles <see cref="GetIoTQuery"/>.</summary>
public sealed class GetIoTQueryHandler : IRequestHandler<GetIoTQuery, Result<IoTQueryResult>>
{
    private readonly IIoTRepository _repository;
    private readonly ILogger<GetIoTQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetIoTQueryHandler(IIoTRepository repository, ILogger<GetIoTQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<IoTQueryResult>> Handle(GetIoTQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetIoTReadingsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<IoTQueryResult>.Success(new IoTQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Integrations.IoT data for Tenant {TenantId}", request.TenantId);

            return Result<IoTQueryResult>.Failure(new ApplicationError(
                "Integrations.IoT.RetrievalFailed",
                "Unable to retrieve IoT reading data at this time.",
                Guid.NewGuid()));
        }
    }
}

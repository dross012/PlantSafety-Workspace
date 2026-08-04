// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for GetDriverQualificationStatusQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.LogisticsTransportSafety.DriverQualifications.Queries;
using PlantSafety.Application.LogisticsTransportSafety.DriverQualifications.Repositories;

namespace PlantSafety.Application.LogisticsTransportSafety.DriverQualifications.Handlers;

/// <summary>Handles <see cref="GetDriverQualificationStatusQuery"/>.</summary>
public sealed class GetDriverQualificationStatusQueryHandler : IRequestHandler<GetDriverQualificationStatusQuery, Result<DriverQualificationQueryResult>>
{
    private readonly IDriverQualificationRepository _repository;
    private readonly ILogger<GetDriverQualificationStatusQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetDriverQualificationStatusQueryHandler(IDriverQualificationRepository repository, ILogger<GetDriverQualificationStatusQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<DriverQualificationQueryResult>> Handle(GetDriverQualificationStatusQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetDriverQualificationStatusAsync(request.TenantId, request.DriverUserId, cancellationToken);

            return Result<DriverQualificationQueryResult>.Success(new DriverQualificationQueryResult(
                request.TenantId,
                request.DriverUserId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve DriverQualification status for Driver {DriverUserId}", request.DriverUserId);

            return Result<DriverQualificationQueryResult>.Failure(new ApplicationError(
                "LogisticsTransportSafety.DriverQualifications.RetrievalFailed",
                "Unable to retrieve DriverQualification status at this time.",
                Guid.NewGuid()));
        }
    }
}

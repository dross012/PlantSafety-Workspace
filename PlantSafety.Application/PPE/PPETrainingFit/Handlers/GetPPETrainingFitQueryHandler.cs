// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetPPETrainingFitQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.PPE.PPETrainingFit.Queries;
using PlantSafety.Application.PPE.PPETrainingFit.Repositories;

namespace PlantSafety.Application.PPE.PPETrainingFit.Handlers;

/// <summary>Handles <see cref="GetPPETrainingFitQuery"/>.</summary>
public sealed class GetPPETrainingFitQueryHandler : IRequestHandler<GetPPETrainingFitQuery, Result<PPETrainingFitQueryResult>>
{
    private readonly IPPETrainingFitRepository _repository;
    private readonly ILogger<GetPPETrainingFitQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetPPETrainingFitQueryHandler(IPPETrainingFitRepository repository, ILogger<GetPPETrainingFitQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<PPETrainingFitQueryResult>> Handle(GetPPETrainingFitQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetPPETrainingFitAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<PPETrainingFitQueryResult>.Success(new PPETrainingFitQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve PPETrainingFit for Tenant {TenantId}", request.TenantId);

            return Result<PPETrainingFitQueryResult>.Failure(new ApplicationError(
                "PPE.PPETrainingFit.RetrievalFailed",
                "Unable to retrieve PPETrainingFit data at this time.",
                Guid.NewGuid()));
        }
    }
}
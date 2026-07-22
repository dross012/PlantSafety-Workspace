// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetStopWorkActionsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.SafetyCulture.CultureAssessment.StopWorkActions.Queries;
using PlantSafety.Application.SafetyCulture.CultureAssessment.StopWorkActions.Repositories;

namespace PlantSafety.Application.SafetyCulture.CultureAssessment.StopWorkActions.Handlers;

/// <summary>Handles <see cref="GetStopWorkActionsQuery"/>.</summary>
public sealed class GetStopWorkActionsQueryHandler : IRequestHandler<GetStopWorkActionsQuery, Result<StopWorkActionsQueryResult>>
{
    private readonly IStopWorkActionsRepository _repository;
    private readonly ILogger<GetStopWorkActionsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetStopWorkActionsQueryHandler(IStopWorkActionsRepository repository, ILogger<GetStopWorkActionsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<StopWorkActionsQueryResult>> Handle(GetStopWorkActionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<StopWorkActionsQueryResult>.Success(new StopWorkActionsQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve StopWorkActions for Tenant {TenantId}", request.TenantId);

            return Result<StopWorkActionsQueryResult>.Failure(new ApplicationError(
                "SafetyCulture.CultureAssessment.StopWorkActions.RetrievalFailed",
                "Unable to retrieve Stop-Work Actions data at this time.",
                Guid.NewGuid()));
        }
    }
}
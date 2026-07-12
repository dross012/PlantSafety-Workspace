// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-13
// FILE DESCRIPTION: Handler for BulkUpdateSafetyEventsCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyEvents.Commands;

/// <summary>Handles <see cref="BulkUpdateSafetyEventsCommand"/>.</summary>
public sealed class BulkUpdateSafetyEventsCommandHandler : IRequestHandler<BulkUpdateSafetyEventsCommand, Result<BulkUpdateSafetyEventsResult>>
{
    private readonly ISafetyEventsBulkRepository _repository;
    private readonly ILogger<BulkUpdateSafetyEventsCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public BulkUpdateSafetyEventsCommandHandler(ISafetyEventsBulkRepository repository, ILogger<BulkUpdateSafetyEventsCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<BulkUpdateSafetyEventsResult>> Handle(BulkUpdateSafetyEventsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var results = await _repository.BulkUpdateAsync(
                request.TenantId,
                request.SiteId,
                request.ResourceIds,
                request.Operation,
                request.Parameters,
                cancellationToken);

            var succeeded = results.Count(r => r.Success);
            var failed = results.Count - succeeded;

            return Result<BulkUpdateSafetyEventsResult>.Success(new BulkUpdateSafetyEventsResult(succeeded, failed, results));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute bulk SafetyEvents {Operation} for Tenant {TenantId}", request.Operation, request.TenantId);

            return Result<BulkUpdateSafetyEventsResult>.Failure(new ApplicationError(
                "SafetyEvents.Bulk.OperationFailed",
                "Unable to complete the bulk operation at this time.",
                Guid.NewGuid()));
        }
    }
}

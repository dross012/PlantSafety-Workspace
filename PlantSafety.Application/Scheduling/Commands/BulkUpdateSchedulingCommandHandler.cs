// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for BulkUpdateSchedulingCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Scheduling.Commands;

/// <summary>Handles <see cref="BulkUpdateSchedulingCommand"/>.</summary>
public sealed class BulkUpdateSchedulingCommandHandler : IRequestHandler<BulkUpdateSchedulingCommand, Result<BulkUpdateSchedulingResult>>
{
    private readonly ISchedulingBulkRepository _repository;
    private readonly ILogger<BulkUpdateSchedulingCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public BulkUpdateSchedulingCommandHandler(ISchedulingBulkRepository repository, ILogger<BulkUpdateSchedulingCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<BulkUpdateSchedulingResult>> Handle(BulkUpdateSchedulingCommand request, CancellationToken cancellationToken)
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

            return Result<BulkUpdateSchedulingResult>.Success(new BulkUpdateSchedulingResult(succeeded, failed, results));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute bulk Scheduling {Operation} for Tenant {TenantId}", request.Operation, request.TenantId);

            return Result<BulkUpdateSchedulingResult>.Failure(new ApplicationError(
                "Scheduling.Bulk.OperationFailed",
                "Unable to complete the bulk operation at this time.",
                Guid.NewGuid()));
        }
    }
}
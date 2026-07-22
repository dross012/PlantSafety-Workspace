// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for BulkUpdateObservabilityCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Observability.Commands;

/// <summary>Handles <see cref="BulkUpdateObservabilityCommand"/>.</summary>
public sealed class BulkUpdateObservabilityCommandHandler : IRequestHandler<BulkUpdateObservabilityCommand, Result<BulkUpdateObservabilityResult>>
{
    private readonly IObservabilityBulkRepository _repository;
    private readonly ILogger<BulkUpdateObservabilityCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public BulkUpdateObservabilityCommandHandler(IObservabilityBulkRepository repository, ILogger<BulkUpdateObservabilityCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<BulkUpdateObservabilityResult>> Handle(BulkUpdateObservabilityCommand request, CancellationToken cancellationToken)
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

            return Result<BulkUpdateObservabilityResult>.Success(new BulkUpdateObservabilityResult(succeeded, failed, results));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute bulk Observability {Operation} for Tenant {TenantId}", request.Operation, request.TenantId);

            return Result<BulkUpdateObservabilityResult>.Failure(new ApplicationError(
                "Observability.Bulk.OperationFailed",
                "Unable to complete the bulk operation at this time.",
                Guid.NewGuid()));
        }
    }
}
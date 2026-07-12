// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-13
// FILE DESCRIPTION: Handler for BulkUpdateOperationalCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Operational.Commands;

/// <summary>Handles <see cref="BulkUpdateOperationalCommand"/>.</summary>
public sealed class BulkUpdateOperationalCommandHandler : IRequestHandler<BulkUpdateOperationalCommand, Result<BulkUpdateOperationalResult>>
{
    private readonly IOperationalBulkRepository _repository;
    private readonly ILogger<BulkUpdateOperationalCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public BulkUpdateOperationalCommandHandler(IOperationalBulkRepository repository, ILogger<BulkUpdateOperationalCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<BulkUpdateOperationalResult>> Handle(BulkUpdateOperationalCommand request, CancellationToken cancellationToken)
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

            return Result<BulkUpdateOperationalResult>.Success(new BulkUpdateOperationalResult(succeeded, failed, results));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute bulk Operational {Operation} for Tenant {TenantId}", request.Operation, request.TenantId);

            return Result<BulkUpdateOperationalResult>.Failure(new ApplicationError(
                "Operational.Bulk.OperationFailed",
                "Unable to complete the bulk operation at this time.",
                Guid.NewGuid()));
        }
    }
}

// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for BulkUpdatePermitToWorkCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PermitToWork.Commands;

/// <summary>Handles <see cref="BulkUpdatePermitToWorkCommand"/>.</summary>
public sealed class BulkUpdatePermitToWorkCommandHandler : IRequestHandler<BulkUpdatePermitToWorkCommand, Result<BulkUpdatePermitToWorkResult>>
{
    private readonly IPermitToWorkBulkRepository _repository;
    private readonly ILogger<BulkUpdatePermitToWorkCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public BulkUpdatePermitToWorkCommandHandler(IPermitToWorkBulkRepository repository, ILogger<BulkUpdatePermitToWorkCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<BulkUpdatePermitToWorkResult>> Handle(BulkUpdatePermitToWorkCommand request, CancellationToken cancellationToken)
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

            return Result<BulkUpdatePermitToWorkResult>.Success(new BulkUpdatePermitToWorkResult(succeeded, failed, results));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute bulk PermitToWork {Operation} for Tenant {TenantId}", request.Operation, request.TenantId);

            return Result<BulkUpdatePermitToWorkResult>.Failure(new ApplicationError(
                "PermitToWork.Bulk.OperationFailed",
                "Unable to complete the bulk operation at this time.",
                Guid.NewGuid()));
        }
    }
}
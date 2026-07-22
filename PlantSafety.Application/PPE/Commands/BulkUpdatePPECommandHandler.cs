// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for BulkUpdatePPECommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PPE.Commands;

/// <summary>Handles <see cref="BulkUpdatePPECommand"/>.</summary>
public sealed class BulkUpdatePPECommandHandler : IRequestHandler<BulkUpdatePPECommand, Result<BulkUpdatePPEResult>>
{
    private readonly IPPEBulkRepository _repository;
    private readonly ILogger<BulkUpdatePPECommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public BulkUpdatePPECommandHandler(IPPEBulkRepository repository, ILogger<BulkUpdatePPECommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<BulkUpdatePPEResult>> Handle(BulkUpdatePPECommand request, CancellationToken cancellationToken)
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

            return Result<BulkUpdatePPEResult>.Success(new BulkUpdatePPEResult(succeeded, failed, results));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute bulk PPE {Operation} for Tenant {TenantId}", request.Operation, request.TenantId);

            return Result<BulkUpdatePPEResult>.Failure(new ApplicationError(
                "PPE.Bulk.OperationFailed",
                "Unable to complete the bulk operation at this time.",
                Guid.NewGuid()));
        }
    }
}
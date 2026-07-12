// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-13
// FILE DESCRIPTION: Handler for BulkUpdateComplianceCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.Compliance.Commands;

/// <summary>Handles <see cref="BulkUpdateComplianceCommand"/>.</summary>
public sealed class BulkUpdateComplianceCommandHandler : IRequestHandler<BulkUpdateComplianceCommand, Result<BulkUpdateComplianceResult>>
{
    private readonly IComplianceBulkRepository _repository;
    private readonly ILogger<BulkUpdateComplianceCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public BulkUpdateComplianceCommandHandler(IComplianceBulkRepository repository, ILogger<BulkUpdateComplianceCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<BulkUpdateComplianceResult>> Handle(BulkUpdateComplianceCommand request, CancellationToken cancellationToken)
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

            return Result<BulkUpdateComplianceResult>.Success(new BulkUpdateComplianceResult(succeeded, failed, results));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute bulk Compliance {Operation} for Tenant {TenantId}", request.Operation, request.TenantId);

            return Result<BulkUpdateComplianceResult>.Failure(new ApplicationError(
                "Compliance.Bulk.OperationFailed",
                "Unable to complete the bulk operation at this time.",
                Guid.NewGuid()));
        }
    }
}

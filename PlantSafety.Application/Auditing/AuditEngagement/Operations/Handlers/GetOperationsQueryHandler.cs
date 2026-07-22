// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetOperationsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Auditing.AuditEngagement.Operations.Queries;
using PlantSafety.Application.Auditing.AuditEngagement.Operations.Repositories;

namespace PlantSafety.Application.Auditing.AuditEngagement.Operations.Handlers;

/// <summary>Handles <see cref="GetOperationsQuery"/>.</summary>
public sealed class GetOperationsQueryHandler : IRequestHandler<GetOperationsQuery, Result<OperationsQueryResult>>
{
    private readonly IOperationsRepository _repository;
    private readonly ILogger<GetOperationsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetOperationsQueryHandler(IOperationsRepository repository, ILogger<GetOperationsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<OperationsQueryResult>> Handle(GetOperationsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<OperationsQueryResult>.Success(new OperationsQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Operations for Tenant {TenantId}", request.TenantId);

            return Result<OperationsQueryResult>.Failure(new ApplicationError(
                "Auditing.AuditEngagement.Operations.RetrievalFailed",
                "Unable to retrieve Operations Auditing data at this time.",
                Guid.NewGuid()));
        }
    }
}
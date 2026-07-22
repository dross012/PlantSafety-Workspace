// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetQualityControlQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Auditing.AuditEngagement.QualityControl.Queries;
using PlantSafety.Application.Auditing.AuditEngagement.QualityControl.Repositories;

namespace PlantSafety.Application.Auditing.AuditEngagement.QualityControl.Handlers;

/// <summary>Handles <see cref="GetQualityControlQuery"/>.</summary>
public sealed class GetQualityControlQueryHandler : IRequestHandler<GetQualityControlQuery, Result<QualityControlQueryResult>>
{
    private readonly IQualityControlRepository _repository;
    private readonly ILogger<GetQualityControlQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetQualityControlQueryHandler(IQualityControlRepository repository, ILogger<GetQualityControlQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<QualityControlQueryResult>> Handle(GetQualityControlQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<QualityControlQueryResult>.Success(new QualityControlQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve QualityControl for Tenant {TenantId}", request.TenantId);

            return Result<QualityControlQueryResult>.Failure(new ApplicationError(
                "Auditing.AuditEngagement.QualityControl.RetrievalFailed",
                "Unable to retrieve Quality Control Auditing data at this time.",
                Guid.NewGuid()));
        }
    }
}
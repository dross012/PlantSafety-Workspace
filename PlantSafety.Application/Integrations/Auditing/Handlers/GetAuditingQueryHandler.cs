// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Handler for GetAuditingQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Integrations.Auditing.Queries;
using PlantSafety.Application.Integrations.Auditing.Repositories;

namespace PlantSafety.Application.Integrations.Auditing.Handlers;

/// <summary>Handles <see cref="GetAuditingQuery"/>.</summary>
public sealed class GetAuditingQueryHandler : IRequestHandler<GetAuditingQuery, Result<AuditingQueryResult>>
{
    private readonly IAuditingRepository _repository;
    private readonly ILogger<GetAuditingQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetAuditingQueryHandler(IAuditingRepository repository, ILogger<GetAuditingQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AuditingQueryResult>> Handle(GetAuditingQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetLogsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<AuditingQueryResult>.Success(new AuditingQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Integrations.Auditing for Tenant {TenantId}", request.TenantId);

            return Result<AuditingQueryResult>.Failure(new ApplicationError(
                "Integrations.Auditing.RetrievalFailed",
                "Unable to retrieve integration audit log entries at this time.",
                Guid.NewGuid()));
        }
    }
}

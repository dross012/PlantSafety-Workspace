// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetInternalSafetyAuditQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Auditing.InternalSafetyAudit.Queries;
using PlantSafety.Application.Compliance.Auditing.InternalSafetyAudit.Repositories;

namespace PlantSafety.Application.Compliance.Auditing.InternalSafetyAudit.Handlers;

/// <summary>Handles <see cref="GetInternalSafetyAuditQuery"/>.</summary>
public sealed class GetInternalSafetyAuditQueryHandler : IRequestHandler<GetInternalSafetyAuditQuery, Result<InternalSafetyAuditQueryResult>>
{
    private readonly IInternalSafetyAuditRepository _repository;
    private readonly ILogger<GetInternalSafetyAuditQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetInternalSafetyAuditQueryHandler(IInternalSafetyAuditRepository repository, ILogger<GetInternalSafetyAuditQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<InternalSafetyAuditQueryResult>> Handle(GetInternalSafetyAuditQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetAuditsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<InternalSafetyAuditQueryResult>.Success(new InternalSafetyAuditQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve InternalSafetyAudit for Tenant {TenantId}", request.TenantId);

            return Result<InternalSafetyAuditQueryResult>.Failure(new ApplicationError(
                "Compliance.Auditing.InternalSafetyAudit.RetrievalFailed",
                "Unable to retrieve internal safety audit data at this time.",
                Guid.NewGuid()));
        }
    }
}

// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetAuditFindingsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Compliance.Auditing.AuditFindings.Queries;
using PlantSafety.Application.Compliance.Auditing.AuditFindings.Repositories;

namespace PlantSafety.Application.Compliance.Auditing.AuditFindings.Handlers;

/// <summary>Handles <see cref="GetAuditFindingsQuery"/>.</summary>
public sealed class GetAuditFindingsQueryHandler : IRequestHandler<GetAuditFindingsQuery, Result<AuditFindingsQueryResult>>
{
    private readonly IAuditFindingsRepository _repository;
    private readonly ILogger<GetAuditFindingsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetAuditFindingsQueryHandler(IAuditFindingsRepository repository, ILogger<GetAuditFindingsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AuditFindingsQueryResult>> Handle(GetAuditFindingsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetFindingsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<AuditFindingsQueryResult>.Success(new AuditFindingsQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve AuditFindings for Tenant {TenantId}", request.TenantId);

            return Result<AuditFindingsQueryResult>.Failure(new ApplicationError(
                "Compliance.Auditing.AuditFindings.RetrievalFailed",
                "Unable to retrieve audit findings at this time.",
                Guid.NewGuid()));
        }
    }
}

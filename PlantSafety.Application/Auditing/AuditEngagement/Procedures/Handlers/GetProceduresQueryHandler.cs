// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for GetProceduresQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Auditing.AuditEngagement.Procedures.Queries;
using PlantSafety.Application.Auditing.AuditEngagement.Procedures.Repositories;

namespace PlantSafety.Application.Auditing.AuditEngagement.Procedures.Handlers;

/// <summary>Handles <see cref="GetProceduresQuery"/>.</summary>
public sealed class GetProceduresQueryHandler : IRequestHandler<GetProceduresQuery, Result<ProceduresQueryResult>>
{
    private readonly IProceduresRepository _repository;
    private readonly ILogger<GetProceduresQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetProceduresQueryHandler(IProceduresRepository repository, ILogger<GetProceduresQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<ProceduresQueryResult>> Handle(GetProceduresQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<ProceduresQueryResult>.Success(new ProceduresQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Procedures for Tenant {TenantId}", request.TenantId);

            return Result<ProceduresQueryResult>.Failure(new ApplicationError(
                "Auditing.AuditEngagement.Procedures.RetrievalFailed",
                "Unable to retrieve Procedures Auditing data at this time.",
                Guid.NewGuid()));
        }
    }
}
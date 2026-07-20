// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-20
// FILE DESCRIPTION: Handler for SearchAuditLogQuery.
// SchemaVersion: 1

using System;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Operations.AuditViewer.Models;
using PlantSafety.Application.Operations.AuditViewer.Queries;
using PlantSafety.Application.Operations.AuditViewer.Repositories;

namespace PlantSafety.Application.Operations.AuditViewer.Handlers;

/// <summary>Handles <see cref="SearchAuditLogQuery"/>.</summary>
public sealed class SearchAuditLogQueryHandler : IRequestHandler<SearchAuditLogQuery, Result<AuditLogSearchResultDto>>
{
    private readonly IAuditLogRepository _repository;
    private readonly ILogger<SearchAuditLogQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public SearchAuditLogQueryHandler(IAuditLogRepository repository, ILogger<SearchAuditLogQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<AuditLogSearchResultDto>> Handle(SearchAuditLogQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _repository.SearchAsync(request.Criteria, cancellationToken);
            return Result<AuditLogSearchResultDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search AuditLog with criteria {@Criteria}.", request.Criteria);

            return Result<AuditLogSearchResultDto>.Failure(new ApplicationError(
                "Operations.AuditViewer.SearchFailed",
                "Unable to search the audit log at this time.",
                Guid.NewGuid()));
        }
    }
}

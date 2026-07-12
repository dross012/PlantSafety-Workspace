// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Handler for GetBackupQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.CrossDomain.Backup.Queries;
using PlantSafety.Application.CrossDomain.Backup.Repositories;

namespace PlantSafety.Application.CrossDomain.Backup.Handlers;

/// <summary>Handles <see cref="GetBackupQuery"/>.</summary>
public sealed class GetBackupQueryHandler : IRequestHandler<GetBackupQuery, Result<BackupQueryResult>>
{
    private readonly IBackupRepository _repository;
    private readonly ILogger<GetBackupQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetBackupQueryHandler(IBackupRepository repository, ILogger<GetBackupQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<BackupQueryResult>> Handle(GetBackupQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetBackupJobsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<BackupQueryResult>.Success(new BackupQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Backup for Tenant {TenantId}", request.TenantId);

            return Result<BackupQueryResult>.Failure(new ApplicationError(
                "CrossDomain.Backup.RetrievalFailed",
                "Unable to retrieve backup data at this time.",
                Guid.NewGuid()));
        }
    }
}

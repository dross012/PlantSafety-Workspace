// AUTHOR: PlantSafety Platform Team (Claude Code modular-creation-agent)
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for GetCommunityMeetingRecordsQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.CommunitySafety.CommunityMeetingRecord.Queries;
using PlantSafety.Application.CommunitySafety.CommunityMeetingRecord.Repositories;

namespace PlantSafety.Application.CommunitySafety.CommunityMeetingRecord.Handlers;

/// <summary>Handles <see cref="GetCommunityMeetingRecordsQuery"/>.</summary>
public sealed class GetCommunityMeetingRecordsQueryHandler : IRequestHandler<GetCommunityMeetingRecordsQuery, Result<CommunityMeetingRecordsQueryResult>>
{
    private readonly ICommunityMeetingRecordRepository _repository;
    private readonly ILogger<GetCommunityMeetingRecordsQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetCommunityMeetingRecordsQueryHandler(ICommunityMeetingRecordRepository repository, ILogger<GetCommunityMeetingRecordsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CommunityMeetingRecordsQueryResult>> Handle(GetCommunityMeetingRecordsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetCommunityMeetingRecordItemsAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<CommunityMeetingRecordsQueryResult>.Success(new CommunityMeetingRecordsQueryResult(
                request.TenantId, request.SiteId, DateTime.UtcNow, items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve CommunityMeetingRecord records for Tenant {TenantId}", request.TenantId);

            return Result<CommunityMeetingRecordsQueryResult>.Failure(new ApplicationError(
                "CommunitySafety.CommunityMeetingRecord.RetrievalFailed",
                "Unable to retrieve CommunityMeetingRecord records at this time.",
                Guid.NewGuid()));
        }
    }
}

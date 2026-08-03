// AUTHOR: PlantSafety Platform Team (Phase 7 §6.3 Support Tooling)
// CREATED: 2026-08-03
// FILE DESCRIPTION: Handler for GetActiveImpersonationSessionQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Abstractions.Caching;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Support.Impersonation.Models;
using PlantSafety.Application.Support.Impersonation.Queries;

namespace PlantSafety.Application.Support.Impersonation.Handlers;

/// <summary>Handles <see cref="GetActiveImpersonationSessionQuery"/>.</summary>
public sealed class GetActiveImpersonationSessionQueryHandler(
    ICacheService cacheService,
    ILogger<GetActiveImpersonationSessionQueryHandler> logger)
    : IRequestHandler<GetActiveImpersonationSessionQuery, Result<ImpersonationSessionDto>>
{
    /// <inheritdoc/>
    public async Task<Result<ImpersonationSessionDto>> Handle(GetActiveImpersonationSessionQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var session = await cacheService.GetAsync<ImpersonationSessionDto>(
                ImpersonationCacheKeys.ForSession(request.SessionId), cancellationToken);

            if (session is null)
            {
                return Result<ImpersonationSessionDto>.Failure(new ApplicationError(
                    "Support.Impersonation.SessionNotFound", "No active impersonation session was found (it may have already ended or expired).", Guid.NewGuid()));
            }

            return Result<ImpersonationSessionDto>.Success(session);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve impersonation session {SessionId}", request.SessionId);

            return Result<ImpersonationSessionDto>.Failure(new ApplicationError(
                "Support.Impersonation.RetrievalFailed", "Unable to retrieve the impersonation session at this time.", Guid.NewGuid()));
        }
    }
}

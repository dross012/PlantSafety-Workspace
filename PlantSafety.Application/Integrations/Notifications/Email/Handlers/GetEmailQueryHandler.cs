// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-12
// FILE DESCRIPTION: Handler for GetEmailQuery.
// SchemaVersion: 1
using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.Integrations.Notifications.Email.Queries;
using PlantSafety.Application.Integrations.Notifications.Email.Repositories;

namespace PlantSafety.Application.Integrations.Notifications.Email.Handlers;

/// <summary>Handles <see cref="GetEmailQuery"/>.</summary>
public sealed class GetEmailQueryHandler : IRequestHandler<GetEmailQuery, Result<EmailQueryResult>>
{
    private readonly IEmailRepository _repository;
    private readonly ILogger<GetEmailQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetEmailQueryHandler(IEmailRepository repository, ILogger<GetEmailQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<EmailQueryResult>> Handle(GetEmailQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var items = await _repository.GetEmailDeliveriesAsync(request.TenantId, request.SiteId, request.From, request.To, cancellationToken);

            return Result<EmailQueryResult>.Success(new EmailQueryResult(
                request.TenantId,
                request.SiteId,
                DateTime.UtcNow,
                items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Email data for Tenant {TenantId}", request.TenantId);

            return Result<EmailQueryResult>.Failure(new ApplicationError(
                "Integrations.Notifications.Email.RetrievalFailed",
                "Unable to retrieve Email data at this time.",
                Guid.NewGuid()));
        }
    }
}
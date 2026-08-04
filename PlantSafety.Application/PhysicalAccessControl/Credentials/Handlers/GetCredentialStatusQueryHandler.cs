// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for GetCredentialStatusQuery.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;
using PlantSafety.Application.PhysicalAccessControl.Credentials.Queries;
using PlantSafety.Application.PhysicalAccessControl.Credentials.Repositories;

namespace PlantSafety.Application.PhysicalAccessControl.Credentials.Handlers;

/// <summary>Handles <see cref="GetCredentialStatusQuery"/>.</summary>
public sealed class GetCredentialStatusQueryHandler : IRequestHandler<GetCredentialStatusQuery, Result<GetCredentialStatusResult>>
{
    private readonly ICredentialReadRepository _repository;
    private readonly ILogger<GetCredentialStatusQueryHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public GetCredentialStatusQueryHandler(ICredentialReadRepository repository, ILogger<GetCredentialStatusQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<GetCredentialStatusResult>> Handle(GetCredentialStatusQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var credential = await _repository.GetStatusAsync(request.TenantId, request.CredentialId, cancellationToken);

            return Result<GetCredentialStatusResult>.Success(new GetCredentialStatusResult(credential));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve Credential status {CredentialId} for Tenant {TenantId}", request.CredentialId, request.TenantId);

            return Result<GetCredentialStatusResult>.Failure(new ApplicationError(
                "PhysicalAccessControl.Credential.RetrievalFailed",
                "Unable to retrieve the Credential status at this time.",
                Guid.NewGuid()));
        }
    }
}

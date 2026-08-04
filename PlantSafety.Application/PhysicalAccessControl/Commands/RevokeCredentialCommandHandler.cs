// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-08-04
// FILE DESCRIPTION: Handler for RevokeCredentialCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.PhysicalAccessControl.Credentials.Interfaces;
using PlantSafety.Domain.PhysicalAccessControl.Credentials.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.PhysicalAccessControl.Commands;

/// <summary>Handles <see cref="RevokeCredentialCommand"/>.</summary>
public sealed class RevokeCredentialCommandHandler : IRequestHandler<RevokeCredentialCommand, Result<CredentialActionResult>>
{
    private readonly ICredentialRepository _repository;
    private readonly ILogger<RevokeCredentialCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public RevokeCredentialCommandHandler(ICredentialRepository repository, ILogger<RevokeCredentialCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<CredentialActionResult>> Handle(RevokeCredentialCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = CredentialId.FromGuid(request.CredentialId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<CredentialActionResult>.Failure(new ApplicationError(
                    "PhysicalAccessControl.Credential.NotFound",
                    "The requested Credential record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.Revoke(
                request.Reason,
                UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<CredentialActionResult>.Success(new CredentialActionResult(
                aggregate.CredentialId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute Revoke for Credential {CredentialId}", request.CredentialId);

            return Result<CredentialActionResult>.Failure(new ApplicationError(
                "PhysicalAccessControl.Credential.RevokeFailed",
                "Unable to revoke the Credential record at this time.",
                Guid.NewGuid()));
        }
    }
}

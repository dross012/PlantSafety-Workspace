// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-07-21
// FILE DESCRIPTION: Handler for ExecuteInspectionMachineGuardingInspectionsCommand.
// SchemaVersion: 1

using MediatR;
using Microsoft.Extensions.Logging;
using PlantSafety.SharedKernel.Identifiers;
using PlantSafety.SharedKernel.Primitives.ValueTypes;
using PlantSafety.Domain.SafetyProcedures.MachineGuardingInspections.Interfaces;
using PlantSafety.Domain.SafetyProcedures.MachineGuardingInspections.ValueObjects.Ids;
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.Common.Results;

namespace PlantSafety.Application.SafetyProcedures.Commands;

/// <summary>Handles <see cref="ExecuteInspectionMachineGuardingInspectionsCommand"/>.</summary>
public sealed class ExecuteInspectionMachineGuardingInspectionsCommandHandler : IRequestHandler<ExecuteInspectionMachineGuardingInspectionsCommand, Result<MachineGuardingInspectionsActionResult>>
{
    private readonly IMachineGuardingInspectionsRepository _repository;
    private readonly ILogger<ExecuteInspectionMachineGuardingInspectionsCommandHandler> _logger;

    /// <summary>Initialises dependencies.</summary>
    public ExecuteInspectionMachineGuardingInspectionsCommandHandler(IMachineGuardingInspectionsRepository repository, ILogger<ExecuteInspectionMachineGuardingInspectionsCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<MachineGuardingInspectionsActionResult>> Handle(ExecuteInspectionMachineGuardingInspectionsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var id = MachineGuardingInspectionsId.FromGuid(request.MachineGuardingInspectionsId);
            var aggregate = await _repository.GetByIdAsync(id, cancellationToken);
            if (aggregate is null)
            {
                return Result<MachineGuardingInspectionsActionResult>.Failure(new ApplicationError(
                    "SafetyProcedures.MachineGuardingInspections.NotFound",
                    "The requested MachineGuardingInspections record could not be found.",
                    Guid.NewGuid()));
            }

            var performedBy = UserId.FromGuid(request.PerformedByUserId);

            aggregate.ExecuteInspection(
                request.DeficiencyCount,
                request.ComplianceScore,
                UserId.FromGuid(request.InspectedByUserId),
                UtcTimestamp.Now(),
                actor: PerformedByActor.User(performedBy),
                correlationId: new CorrelationId(request.CorrelationId),
                causationId: new CausationId(request.CausationId));

            await _repository.UpdateAsync(aggregate, cancellationToken);

            return Result<MachineGuardingInspectionsActionResult>.Success(new MachineGuardingInspectionsActionResult(
                aggregate.MachineGuardingInspectionsId.Value, aggregate.Status.ToString()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute ExecuteInspection for MachineGuardingInspections {MachineGuardingInspectionsId}", request.MachineGuardingInspectionsId);

            return Result<MachineGuardingInspectionsActionResult>.Failure(new ApplicationError(
                "SafetyProcedures.MachineGuardingInspections.ExecuteInspectionFailed",
                "Unable to ExecuteInspection the MachineGuardingInspections record at this time.",
                Guid.NewGuid()));
        }
    }
}
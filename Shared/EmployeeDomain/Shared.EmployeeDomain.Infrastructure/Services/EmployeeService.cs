// ============================================================================
//  Project:        Shared.EmployeeDomain.Infrastructure
//  Module:         Services
//  File:           EmployeeService.cs
//  Created:        2026-08-08
//  Description:    IEmployeeService implementation: loads (or creates) an
//                  EmployeeAggregate, invokes the requested command,
//                  persists the result via IEmployeeRepository, and clears
//                  the aggregate's pending domain events -- nothing
//                  publishes them yet, mirroring Shared.FacilityDomain's
//                  FacilityService precedent.
// ============================================================================
using Shared.EmployeeDomain.Abstractions.Aggregates;
using Shared.EmployeeDomain.Abstractions.Enums;
using Shared.EmployeeDomain.Abstractions.Exposure;
using Shared.EmployeeDomain.Abstractions.Repositories;
using Shared.EmployeeDomain.Abstractions.Training;
using Shared.EmployeeDomain.Abstractions.ValueObjects;

namespace Shared.EmployeeDomain.Infrastructure.Services;

/// <summary><see cref="IEmployeeService"/> implementation over <see cref="IEmployeeRepository"/>.</summary>
public sealed class EmployeeService(IEmployeeRepository repository) : IEmployeeService
{
    /// <inheritdoc />
    public async Task<EmployeeState> CreateEmployeeAsync(string name, EmployeeRole role, CancellationToken cancellationToken = default)
    {
        EmployeeAggregate aggregate = EmployeeAggregate.CreateEmployee(EmployeeId.New(), name, role, DateTime.UtcNow);
        await repository.SaveAsync(aggregate, cancellationToken);
        aggregate.ClearDomainEvents();
        return aggregate.State;
    }

    /// <inheritdoc />
    public async Task<EmployeeState?> UpdateEmployeeMetadataAsync(EmployeeId employeeId, string name, EmployeeRole role, CancellationToken cancellationToken = default) =>
        await ApplyUpdateAsync(employeeId, aggregate => aggregate.UpdateEmployeeMetadata(name, role, DateTime.UtcNow), cancellationToken);

    /// <inheritdoc />
    public async Task<EmployeeState?> UpdateTrainingProfileAsync(EmployeeId employeeId, IReadOnlyList<string> completedTrainingCodes, CancellationToken cancellationToken = default) =>
        await ApplyUpdateAsync(employeeId, aggregate => aggregate.UpdateTrainingProfile(completedTrainingCodes, DateTime.UtcNow), cancellationToken);

    /// <inheritdoc />
    public async Task<EmployeeState?> UpdateCertificationsAsync(EmployeeId employeeId, IReadOnlyList<EmployeeCertification> certifications, CancellationToken cancellationToken = default) =>
        await ApplyUpdateAsync(employeeId, aggregate => aggregate.UpdateCertifications(certifications, DateTime.UtcNow), cancellationToken);

    /// <inheritdoc />
    public async Task<EmployeeState?> RecordExposureAsync(EmployeeId employeeId, EmployeeExposureRecord exposureRecord, CancellationToken cancellationToken = default) =>
        await ApplyUpdateAsync(employeeId, aggregate => aggregate.RecordExposure(exposureRecord, DateTime.UtcNow), cancellationToken);

    /// <inheritdoc />
    public async Task<EmployeeState?> RecordInjuryAsync(EmployeeId employeeId, string description, CancellationToken cancellationToken = default) =>
        await ApplyUpdateAsync(employeeId, aggregate => aggregate.RecordInjury(description, DateTime.UtcNow), cancellationToken);

    /// <inheritdoc />
    public async Task<EmployeeState?> GetEmployeeAsync(EmployeeId employeeId, CancellationToken cancellationToken = default)
    {
        EmployeeAggregate? aggregate = await repository.FindAsync(employeeId, cancellationToken);
        return aggregate?.State;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<EmployeeState>> GetAllEmployeesAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<EmployeeAggregate> aggregates = await repository.GetAllAsync(cancellationToken);
        return aggregates.Select(aggregate => aggregate.State).ToArray();
    }

    private async Task<EmployeeState?> ApplyUpdateAsync(EmployeeId employeeId, Action<EmployeeAggregate> applyCommand, CancellationToken cancellationToken)
    {
        EmployeeAggregate? aggregate = await repository.FindAsync(employeeId, cancellationToken);
        if (aggregate is null)
        {
            return null;
        }

        applyCommand(aggregate);
        await repository.SaveAsync(aggregate, cancellationToken);
        aggregate.ClearDomainEvents();
        return aggregate.State;
    }
}

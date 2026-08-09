// ============================================================================
//  Project:        Shared.EmployeeDomain.Infrastructure
//  Module:         Services
//  File:           EmployeeService.cs
//  Created:        2026-08-08
//  Description:    IEmployeeService implementation: loads (or creates) an
//                  EmployeeAggregate, invokes the requested command,
//                  persists the result via IEmployeeRepository, translates
//                  the aggregate's pending domain events into integration
//                  events, and publishes them through IModuleEventGateway
//                  before clearing them.
//
//                  Phase 4 (Employee domain pass), mirroring the Facility
//                  domain pass's fix: previously used DateTime.UtcNow
//                  directly in every method (bypassing DeterministicMode
//                  for the whole Employee domain) and called
//                  aggregate.ClearDomainEvents() immediately after save
//                  without ever publishing anything -- every Employee
//                  domain event (Created/Updated/TrainingUpdated/
//                  CertificationUpdated) was silently discarded. Now uses
//                  IClock for every timestamp and publishes a translated
//                  integration event (Events/EmployeeIntegrationEvents.cs)
//                  for every domain event the aggregate actually raised,
//                  via PublishIsolatedAsync (Phase 3.4) so a failing
//                  subscriber can't turn an already-successful employee
//                  command into an error.
//
//                  Each domain-event case is dispatched to its own
//                  concretely-typed PublishAsync<TEvent> call rather than
//                  translated through a common IModuleEvent?-typed helper
//                  -- the Facility domain pass found that erasing through
//                  a common interface-typed return value makes C# infer
//                  the generic PublishAsync<TEvent> call's TEvent as the
//                  interface, not the concrete type, so nothing subscribed
//                  to the concrete type would ever receive it (no
//                  exception, no failed-publish log).
// ============================================================================
using Shared.Deterministic.Abstractions.Clock;
using Shared.EmployeeDomain.Abstractions.Aggregates;
using Shared.EmployeeDomain.Abstractions.Enums;
using Shared.EmployeeDomain.Abstractions.Exposure;
using Shared.EmployeeDomain.Abstractions.Repositories;
using Shared.EmployeeDomain.Abstractions.Training;
using Shared.EmployeeDomain.Abstractions.ValueObjects;
using Shared.EmployeeDomain.Infrastructure.Events;
using Shared.ModuleContracts.Abstractions.Events;
using Shared.Telemetry.Abstractions.Correlation;
using Shared.Telemetry.Abstractions.Events;

using DomainEvents = Shared.EmployeeDomain.Abstractions.Events;

namespace Shared.EmployeeDomain.Infrastructure.Services;

/// <summary><see cref="IEmployeeService"/> implementation over <see cref="IEmployeeRepository"/>.</summary>
public sealed class EmployeeService(
    IEmployeeRepository repository,
    IClock clock,
    IModuleEventGateway eventGateway,
    ILogEventWriter logEventWriter,
    ICorrelationContext correlationContext) : IEmployeeService
{
    private const string SourceModule = "Employee";

    /// <inheritdoc />
    public async Task<EmployeeState> CreateEmployeeAsync(string name, EmployeeRole role, CancellationToken cancellationToken = default)
    {
        EmployeeAggregate aggregate = EmployeeAggregate.CreateEmployee(EmployeeId.New(), name, role, clock.UtcNow);
        await repository.SaveAsync(aggregate, cancellationToken);
        await PublishAndClearAsync(aggregate, cancellationToken);
        return aggregate.State;
    }

    /// <inheritdoc />
    public async Task<EmployeeState?> UpdateEmployeeMetadataAsync(EmployeeId employeeId, string name, EmployeeRole role, CancellationToken cancellationToken = default) =>
        await ApplyUpdateAsync(employeeId, aggregate => aggregate.UpdateEmployeeMetadata(name, role, clock.UtcNow), cancellationToken);

    /// <inheritdoc />
    public async Task<EmployeeState?> UpdateTrainingProfileAsync(EmployeeId employeeId, IReadOnlyList<string> completedTrainingCodes, CancellationToken cancellationToken = default) =>
        await ApplyUpdateAsync(employeeId, aggregate => aggregate.UpdateTrainingProfile(completedTrainingCodes, clock.UtcNow), cancellationToken);

    /// <inheritdoc />
    public async Task<EmployeeState?> UpdateCertificationsAsync(EmployeeId employeeId, IReadOnlyList<EmployeeCertification> certifications, CancellationToken cancellationToken = default) =>
        await ApplyUpdateAsync(employeeId, aggregate => aggregate.UpdateCertifications(certifications, clock.UtcNow), cancellationToken);

    /// <inheritdoc />
    public async Task<EmployeeState?> RecordExposureAsync(EmployeeId employeeId, EmployeeExposureRecord exposureRecord, CancellationToken cancellationToken = default) =>
        await ApplyUpdateAsync(employeeId, aggregate => aggregate.RecordExposure(exposureRecord, clock.UtcNow), cancellationToken);

    /// <inheritdoc />
    public async Task<EmployeeState?> RecordInjuryAsync(EmployeeId employeeId, string description, CancellationToken cancellationToken = default) =>
        await ApplyUpdateAsync(employeeId, aggregate => aggregate.RecordInjury(description, clock.UtcNow), cancellationToken);

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
        await PublishAndClearAsync(aggregate, cancellationToken);
        return aggregate.State;
    }

    private async Task PublishAndClearAsync(EmployeeAggregate aggregate, CancellationToken cancellationToken)
    {
        foreach (object domainEvent in aggregate.DomainEvents)
        {
            switch (domainEvent)
            {
                case DomainEvents.EmployeeCreatedEvent e:
                    await PublishAsync(new EmployeeCreatedEvent(SourceModule, e.OccurredAtUtc, e.EmployeeId.Value, e.Name), cancellationToken);
                    break;
                case DomainEvents.EmployeeUpdatedEvent e:
                    await PublishAsync(new EmployeeUpdatedEvent(SourceModule, e.OccurredAtUtc, e.EmployeeId.Value, e.Kind.ToString()), cancellationToken);
                    break;
                case DomainEvents.EmployeeTrainingUpdatedEvent e:
                    await PublishAsync(new EmployeeTrainingUpdatedEvent(SourceModule, e.OccurredAtUtc, e.EmployeeId.Value, e.CompletedTrainingCount), cancellationToken);
                    break;
                case DomainEvents.EmployeeCertificationUpdatedEvent e:
                    await PublishAsync(new EmployeeCertificationUpdatedEvent(SourceModule, e.OccurredAtUtc, e.EmployeeId.Value, e.CertificationCount), cancellationToken);
                    break;
            }
        }

        aggregate.ClearDomainEvents();
    }

    private Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken) where TEvent : IModuleEvent =>
        eventGateway.PublishIsolatedAsync(integrationEvent, logEventWriter, correlationContext.CorrelationId, cancellationToken);
}

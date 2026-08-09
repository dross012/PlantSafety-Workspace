// ============================================================================
//  Project:        Shared.EmployeeDomain.Tests
//  File:           EmployeeRepositoryTests.cs
//  Created:        2026-08-08
//  Description:    Phase C.1 unit tests for EmployeeRepository/EmployeeStore
//                  and EmployeeService's orchestration over them.
// ============================================================================
using FluentAssertions;

using Shared.EmployeeDomain.Abstractions.Aggregates;
using Shared.EmployeeDomain.Abstractions.Enums;
using Shared.EmployeeDomain.Abstractions.Repositories;
using Shared.EmployeeDomain.Abstractions.ValueObjects;
using Shared.EmployeeDomain.Infrastructure.Persistence;
using Shared.EmployeeDomain.Infrastructure.Services;

using Xunit;

namespace Shared.EmployeeDomain.Tests.Repositories;

public sealed class EmployeeRepositoryTests
{
    [Fact]
    public async Task FindAsync_UnknownEmployeeId_ReturnsNull()
    {
        var repository = new EmployeeRepository(new EmployeeStore());

        EmployeeAggregate? result = await repository.FindAsync(EmployeeId.New());

        result.Should().BeNull();
    }

    [Fact]
    public async Task SaveAsync_ThenFindAsync_RoundTripsState()
    {
        var repository = new EmployeeRepository(new EmployeeStore());
        EmployeeAggregate created = EmployeeAggregate.CreateEmployee(EmployeeId.New(), "Jane Doe", EmployeeRole.Operator, DateTime.UtcNow);

        await repository.SaveAsync(created);
        EmployeeAggregate? found = await repository.FindAsync(created.State.EmployeeId);

        found.Should().NotBeNull();
        found!.State.Should().Be(created.State);
    }

    [Fact]
    public async Task SaveAsync_ExistingEmployeeId_Overwrites()
    {
        var store = new EmployeeStore();
        var repository = new EmployeeRepository(store);
        EmployeeAggregate aggregate = EmployeeAggregate.CreateEmployee(EmployeeId.New(), "Jane Doe", EmployeeRole.Operator, DateTime.UtcNow);
        await repository.SaveAsync(aggregate);

        aggregate.UpdateEmployeeMetadata("Jane Smith", EmployeeRole.Supervisor, DateTime.UtcNow);
        await repository.SaveAsync(aggregate);

        EmployeeAggregate? found = await repository.FindAsync(aggregate.State.EmployeeId);
        found!.State.Name.Should().Be("Jane Smith");
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEveryPersistedEmployee()
    {
        var repository = new EmployeeRepository(new EmployeeStore());
        EmployeeAggregate first = EmployeeAggregate.CreateEmployee(EmployeeId.New(), "Jane Doe", EmployeeRole.Operator, DateTime.UtcNow);
        EmployeeAggregate second = EmployeeAggregate.CreateEmployee(EmployeeId.New(), "John Roe", EmployeeRole.Technician, DateTime.UtcNow);
        await repository.SaveAsync(first);
        await repository.SaveAsync(second);

        IReadOnlyList<EmployeeAggregate> all = await repository.GetAllAsync();

        all.Should().HaveCount(2);
        all.Select(a => a.State.EmployeeId).Should().BeEquivalentTo([first.State.EmployeeId, second.State.EmployeeId]);
    }

    [Fact]
    public async Task EmployeeService_CreateThenRecordInjury_PersistsChange()
    {
        IEmployeeService service = new EmployeeService(new EmployeeRepository(new EmployeeStore()));

        EmployeeState created = await service.CreateEmployeeAsync("Jane Doe", EmployeeRole.Operator);
        EmployeeState? updated = await service.RecordInjuryAsync(created.EmployeeId, "Twisted ankle");

        updated.Should().NotBeNull();
        updated!.InjuryProfile.TotalInjuryCount.Should().Be(1);
    }

    [Fact]
    public async Task EmployeeService_UpdateUnknownEmployee_ReturnsNull()
    {
        IEmployeeService service = new EmployeeService(new EmployeeRepository(new EmployeeStore()));

        EmployeeState? result = await service.UpdateTrainingProfileAsync(EmployeeId.New(), ["HAZWOPER"]);

        result.Should().BeNull();
    }

    [Fact]
    public async Task EmployeeService_GetAllEmployeesAsync_ReflectsCreatedEmployees()
    {
        IEmployeeService service = new EmployeeService(new EmployeeRepository(new EmployeeStore()));
        await service.CreateEmployeeAsync("Jane Doe", EmployeeRole.Operator);
        await service.CreateEmployeeAsync("John Roe", EmployeeRole.Technician);

        IReadOnlyList<EmployeeState> all = await service.GetAllEmployeesAsync();

        all.Should().HaveCount(2);
    }
}

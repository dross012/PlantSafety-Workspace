// ============================================================================
//  Project:        Shared.EmployeeDomain.Tests
//  File:           EmployeeServiceEventPublishingTests.cs
//  Created:        2026-08-10
//  Description:    Phase 4 (Employee domain pass), Section 9 "Domain
//                  Testing": proves EmployeeService actually publishes an
//                  integration event, of the correct concrete type, for
//                  every command -- through a real ModuleEventGateway, not
//                  a mock, so a subscriber registered via
//                  Subscribe<TConcreteEvent>() actually receives it.
//                  Mirrors Shared.FacilityDomain.Tests'
//                  FacilityServiceEventPublishingTests.cs, added there
//                  after a real bug (generic type inference erasing the
//                  concrete event type when routed through a common
//                  IModuleEvent?-typed helper) was caught only by live
//                  `dotnet run` verification, not by a compile error.
// ============================================================================
using FluentAssertions;

using Shared.Deterministic.Abstractions.Clock;
using Shared.EmployeeDomain.Abstractions.Aggregates;
using Shared.EmployeeDomain.Abstractions.Enums;
using Shared.EmployeeDomain.Abstractions.Repositories;
using Shared.EmployeeDomain.Infrastructure.Events;
using Shared.EmployeeDomain.Infrastructure.Persistence;
using Shared.EmployeeDomain.Infrastructure.Services;
using Shared.ModuleContracts.Abstractions.Events;
using Shared.Telemetry.Abstractions.Correlation;
using Shared.Telemetry.Abstractions.Events;

using Xunit;

namespace Shared.EmployeeDomain.Tests.Services;

public sealed class EmployeeServiceEventPublishingTests
{
    private sealed class NoOpLogEventWriter : ILogEventWriter
    {
        public void Write(ILogEvent logEvent)
        {
        }
    }

    private sealed class FixedCorrelationContext : ICorrelationContext
    {
        public string CorrelationId => "test-correlation";
    }

    private static (EmployeeService Service, IModuleEventGateway Gateway) CreateService()
    {
        var gateway = new ModuleEventGateway();
        var repository = new EmployeeRepository(new EmployeeStore());
        var service = new EmployeeService(repository, new SystemClock(), gateway, new NoOpLogEventWriter(), new FixedCorrelationContext());
        return (service, gateway);
    }

    [Fact]
    public async Task CreateEmployeeAsync_PublishesEmployeeCreatedEvent_ToAConcretelyTypedSubscriber()
    {
        (EmployeeService service, IModuleEventGateway gateway) = CreateService();
        EmployeeCreatedEvent? received = null;
        using IDisposable subscription = gateway.Subscribe<EmployeeCreatedEvent>((evt, _) => { received = evt; return Task.CompletedTask; });

        await service.CreateEmployeeAsync("Jane Doe", EmployeeRole.Operator);

        received.Should().NotBeNull();
        received!.Name.Should().Be("Jane Doe");
        received.SourceModule.Should().Be("Employee");
    }

    [Fact]
    public async Task UpdateEmployeeMetadataAsync_PublishesEmployeeUpdatedEvent()
    {
        (EmployeeService service, IModuleEventGateway gateway) = CreateService();
        EmployeeState created = await service.CreateEmployeeAsync("Jane Doe", EmployeeRole.Operator);
        var received = new List<EmployeeUpdatedEvent>();
        using IDisposable subscription = gateway.Subscribe<EmployeeUpdatedEvent>((evt, _) => { received.Add(evt); return Task.CompletedTask; });

        await service.UpdateEmployeeMetadataAsync(created.EmployeeId, "Jane Smith", EmployeeRole.Supervisor);

        received.Should().Contain(e => e.UpdateKind == "Metadata");
    }

    [Fact]
    public async Task UpdateTrainingProfileAsync_PublishesEmployeeTrainingUpdatedEvent()
    {
        (EmployeeService service, IModuleEventGateway gateway) = CreateService();
        EmployeeState created = await service.CreateEmployeeAsync("Jane Doe", EmployeeRole.Operator);
        EmployeeTrainingUpdatedEvent? received = null;
        using IDisposable subscription = gateway.Subscribe<EmployeeTrainingUpdatedEvent>((evt, _) => { received = evt; return Task.CompletedTask; });

        await service.UpdateTrainingProfileAsync(created.EmployeeId, ["HAZWOPER", "OSHA-10"]);

        received.Should().NotBeNull();
        received!.CompletedTrainingCount.Should().Be(2);
    }

    [Fact]
    public async Task UpdateCertificationsAsync_PublishesEmployeeCertificationUpdatedEvent()
    {
        (EmployeeService service, IModuleEventGateway gateway) = CreateService();
        EmployeeState created = await service.CreateEmployeeAsync("Jane Doe", EmployeeRole.Operator);
        EmployeeCertificationUpdatedEvent? received = null;
        using IDisposable subscription = gateway.Subscribe<EmployeeCertificationUpdatedEvent>((evt, _) => { received = evt; return Task.CompletedTask; });

        await service.UpdateCertificationsAsync(created.EmployeeId, [new Shared.EmployeeDomain.Abstractions.Training.EmployeeCertification("FORKLIFT-OP", "OSHA", DateTime.UtcNow, null)]);

        received.Should().NotBeNull();
        received!.CertificationCount.Should().Be(1);
    }

    [Fact]
    public async Task RecordInjuryAsync_PublishesEmployeeUpdatedEvent_ForInjuryProfileAndSafetyScore()
    {
        (EmployeeService service, IModuleEventGateway gateway) = CreateService();
        EmployeeState created = await service.CreateEmployeeAsync("Jane Doe", EmployeeRole.Operator);
        var received = new List<EmployeeUpdatedEvent>();
        using IDisposable subscription = gateway.Subscribe<EmployeeUpdatedEvent>((evt, _) => { received.Add(evt); return Task.CompletedTask; });

        await service.RecordInjuryAsync(created.EmployeeId, "Twisted ankle");

        received.Should().Contain(e => e.UpdateKind == "InjuryProfile");
        received.Should().Contain(e => e.UpdateKind == "SafetyScore");
    }
}

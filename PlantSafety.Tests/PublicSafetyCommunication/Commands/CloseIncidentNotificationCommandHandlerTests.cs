// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.PublicSafetyCommunication.Commands
//  Layer:          Tests (Unit)
//  File:           CloseIncidentNotificationCommandHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Handler tests for CloseIncidentNotificationCommandHandler, including the
//                  regression guard that a missing IncidentNotification maps to
//                  ErrorSeverity.NotFound. Isolated, deterministic, <50ms per test. No I/O.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.PublicSafetyCommunication.Commands;
using PlantSafety.Domain.PublicSafetyCommunication.AgencyDirectory.ValueObjects.Ids;
using PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Enums;
using PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Interfaces;
using PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.ValueObjects.Ids;
using IncidentNotificationAggregate = PlantSafety.Domain.PublicSafetyCommunication.IncidentNotificationDispatch.Aggregates.IncidentNotification;

namespace PlantSafety.Tests.PublicSafetyCommunication.Commands;

public sealed class CloseIncidentNotificationCommandHandlerTests : UnitTestBase
{
    private readonly IIncidentNotificationRepository _repo = Substitute.For<IIncidentNotificationRepository>();

    private CloseIncidentNotificationCommandHandler Sut() => new(_repo, NullLogger<CloseIncidentNotificationCommandHandler>());

    private static IncidentNotificationAggregate DispatchedNotification() =>
        IncidentNotificationAggregate.Create(
            createdBy: TestUserId,
            createdAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            siteId: null,
            incidentId: Guid.NewGuid(),
            publicSafetyAgencyId: PublicSafetyAgencyId.New(),
            channel: PublicSafetyDeliveryChannel.Phone,
            message: "Ammonia release -- HazMat requested.");

    private static CloseIncidentNotificationCommand CommandFor(IncidentNotificationId id, string? eTag) => new(
        CommandId: Guid.NewGuid(),
        CorrelationId: TestCorrelationId.Value,
        CausationId: TestCausationId.Value,
        TenantId: TestTenantId.Value,
        IncidentNotificationId: id.Value)
    { ActorId = TestUserId.Value, ETag = eTag };

    [Fact]
    public async Task Handle_WithValidCommand_ClosesNotificationAndReturnsSuccess()
    {
        var notification = DispatchedNotification();
        _repo.GetByIdAsync(notification.IncidentNotificationId, Arg.Any<CancellationToken>()).Returns(notification);

        var result = await Sut().Handle(CommandFor(notification.IncidentNotificationId, "0x00001"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Closed");
        await _repo.Received(1).UpdateAsync(notification, "0x00001", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNotificationDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetByIdAsync(Arg.Any<IncidentNotificationId>(), Arg.Any<CancellationToken>()).Returns((IncidentNotificationAggregate?)null);

        var result = await Sut().Handle(CommandFor(IncidentNotificationId.New(), null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("PublicSafetyCommunication.IncidentNotification.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound,
            "a missing IncidentNotification must map to HTTP 404 via ResultResponseExtensions.ToActionResult, not the default 500");
    }

    [Fact]
    public async Task Handle_WhenRepositoryReportsConcurrencyConflict_PropagatesException()
    {
        var notification = DispatchedNotification();
        _repo.GetByIdAsync(notification.IncidentNotificationId, Arg.Any<CancellationToken>()).Returns(notification);
        _repo.UpdateAsync(Arg.Any<IncidentNotificationAggregate>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new ConcurrencyConflictException("stale ETag", TestCorrelationId.Value));

        var act = () => Sut().Handle(CommandFor(notification.IncidentNotificationId, "stale"), CancellationToken.None);

        await Assert.ThrowsAsync<ConcurrencyConflictException>(act);
    }
}

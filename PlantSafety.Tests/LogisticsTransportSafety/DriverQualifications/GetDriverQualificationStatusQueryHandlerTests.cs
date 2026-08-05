// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.LogisticsTransportSafety.DriverQualifications
//  Layer:          Tests (Unit)
//  File:           GetDriverQualificationStatusQueryHandlerTests.cs
//  Created:        2026-08-04
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetDriverQualificationStatusQueryHandler. Read-model
//                  repository is mocked with NSubstitute; no I/O, no network, <50ms per test.
// ============================================================================
using PlantSafety.Application.LogisticsTransportSafety.DriverQualifications.Handlers;
using PlantSafety.Application.LogisticsTransportSafety.DriverQualifications.Models;
using PlantSafety.Application.LogisticsTransportSafety.DriverQualifications.Queries;
using PlantSafety.Application.LogisticsTransportSafety.DriverQualifications.Repositories;

namespace PlantSafety.Tests.LogisticsTransportSafety.DriverQualifications;

public sealed class GetDriverQualificationStatusQueryHandlerTests : UnitTestBase
{
    private static readonly Guid DriverUserId = new("00000013-0000-0000-0000-000000000013");
    private readonly IDriverQualificationRepository _repo = Substitute.For<IDriverQualificationRepository>();

    private static DriverQualificationItemDto Item() => new()
    {
        DriverQualificationId = Guid.NewGuid(),
        TenantId = TestTenantId.Value,
        DriverUserId = DriverUserId,
        QualificationType = "CommercialDriverLicense",
        QualificationStatus = "Qualified",
        LicenseNumber = "CDL-1",
        ExpiresAtUtc = FixedUtcNow.AddYears(1),
        CreatedAtUtc = FixedUtcNow,
        CreatedBy = TestUserId.Value,
    };

    [Fact]
    public async Task Handle_WithMatchingItems_ShouldReturnSuccessWithItems()
    {
        _repo.GetDriverQualificationStatusAsync(TestTenantId.Value, DriverUserId, Arg.Any<CancellationToken>())
            .Returns(new List<DriverQualificationItemDto> { Item() });
        var sut = new GetDriverQualificationStatusQueryHandler(_repo, NullLogger<GetDriverQualificationStatusQueryHandler>());

        var result = await sut.Handle(new GetDriverQualificationStatusQuery(TestTenantId.Value, DriverUserId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.DriverUserId.Should().Be(DriverUserId);
    }

    [Fact]
    public async Task Handle_WithNoMatches_ShouldReturnSuccessWithEmptyItems()
    {
        _repo.GetDriverQualificationStatusAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new List<DriverQualificationItemDto>());
        var sut = new GetDriverQualificationStatusQueryHandler(_repo, NullLogger<GetDriverQualificationStatusQueryHandler>());

        var result = await sut.Handle(new GetDriverQualificationStatusQuery(TestTenantId.Value, DriverUserId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldReturnFailure()
    {
        _repo.GetDriverQualificationStatusAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<DriverQualificationItemDto>>>(_ => throw new InvalidOperationException("DB unavailable"));
        var sut = new GetDriverQualificationStatusQueryHandler(_repo, NullLogger<GetDriverQualificationStatusQueryHandler>());

        var result = await sut.Handle(new GetDriverQualificationStatusQuery(TestTenantId.Value, DriverUserId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("LogisticsTransportSafety.DriverQualifications.RetrievalFailed");
    }
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.HumanFactors.CognitiveLoadAnalytics.Handlers
//  Layer:          Tests (Unit)
//  File:           GetTaskComplexityProfileQueryHandlerTests.cs
//  Created:        2026-08-04
//  SchemaVersion:  2026.1
//  Description:    Unit tests for GetTaskComplexityProfileQueryHandler: successful retrieval,
//                   not-found mapping, and unexpected exception mapping. Isolated, deterministic,
//                   <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Application.Common.Errors;
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Handlers;
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Models;
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Queries;
using PlantSafety.Application.HumanFactors.CognitiveLoadAnalytics.Repositories;

namespace PlantSafety.Tests.HumanFactors.CognitiveLoadAnalytics.Handlers;

public sealed class GetTaskComplexityProfileQueryHandlerTests : UnitTestBase
{
    private readonly ICognitiveLoadAnalyticsReadRepository _repo = Substitute.For<ICognitiveLoadAnalyticsReadRepository>();

    private GetTaskComplexityProfileQueryHandler Sut() => new(_repo, NullLogger<GetTaskComplexityProfileQueryHandler>());

    [Fact]
    public async Task Handle_WhenProfileExists_ReturnsProfile()
    {
        var dto = new TaskComplexityProfileDto
        {
            TaskComplexityProfileId = Guid.NewGuid(), CognitiveLoadLedgerId = Guid.NewGuid(),
            TaskCode = "CTRL-ROOM-STARTUP", TaskName = "Control Room Startup", ComplexityScore = 7,
        };
        _repo.GetTaskComplexityProfileAsync(TestTenantId.Value, "CTRL-ROOM-STARTUP", Arg.Any<CancellationToken>()).Returns(dto);

        var result = await Sut().Handle(new GetTaskComplexityProfileQuery(TestTenantId.Value, "CTRL-ROOM-STARTUP"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(dto);
    }

    [Fact]
    public async Task Handle_WhenProfileDoesNotExist_ReturnsNotFoundWithCorrectSeverity()
    {
        _repo.GetTaskComplexityProfileAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((TaskComplexityProfileDto?)null);

        var result = await Sut().Handle(new GetTaskComplexityProfileQuery(TestTenantId.Value, "UNKNOWN"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("HumanFactors.CognitiveLoadAnalytics.TaskComplexityProfile.NotFound");
        result.Error.Severity.Should().Be(ErrorSeverity.NotFound);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ReturnsGenericFailureWithoutLeakingException()
    {
        _repo.GetTaskComplexityProfileAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns<Task<TaskComplexityProfileDto?>>(_ => throw new InvalidOperationException("boom"));

        var result = await Sut().Handle(new GetTaskComplexityProfileQuery(TestTenantId.Value, "X"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorCode.Should().Be("HumanFactors.CognitiveLoadAnalytics.TaskComplexityProfile.RetrievalFailed");
        result.Error.Message.Should().NotContain("boom");
    }
}

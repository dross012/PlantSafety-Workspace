// ============================================================================
//  Project:        Shared.Scheduling.Tests
//  File:           SchedulingDispatcherContractsTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.Scheduling.Abstractions.Contracts;

using Xunit;

namespace Shared.Scheduling.Tests.Contracts;

public sealed class SchedulingDispatcherContractsTests
{
    [Fact]
    public void SchedulingDispatchEnvelope_ImplementsInterfacePropertiesFromPositionalRecord()
    {
        var request = new SchedulingDispatchRequest("ACT-1", "Description", DateTime.UtcNow, IsOverdue: false, "acme-plant-1", FacilityId: null);
        var metadata = new SchedulingDispatchMetadata(SchedulingDispatchMetadata.CurrentContractVersion, "corr-1", DateTime.UtcNow);

        ISchedulingDispatcherContract envelope = new SchedulingDispatchEnvelope(request, metadata);

        envelope.Request.Should().Be(request);
        envelope.Metadata.Should().Be(metadata);
    }

    [Fact]
    public void SchedulingErrors_NoRequiredActionsSupplied_IsUpperSnakeCase()
    {
        SchedulingErrors.NoRequiredActionsSupplied.Should().Be("SCHEDULING_NO_REQUIRED_ACTIONS_SUPPLIED");
    }

    [Fact]
    public void SchedulingValidationException_CarriesErrorCode()
    {
        var exception = new SchedulingValidationException(SchedulingErrors.DuplicateActionCode, "message");

        exception.ErrorCode.Should().Be(SchedulingErrors.DuplicateActionCode);
        exception.Should().BeAssignableTo<ArgumentException>();
    }
}

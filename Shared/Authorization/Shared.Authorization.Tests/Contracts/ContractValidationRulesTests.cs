// ============================================================================
//  Project:        Shared.Authorization.Tests
//  File:           ContractValidationRulesTests.cs
//  Created:        2026-08-10
// ============================================================================
using AwesomeAssertions;

using Shared.Authorization.Abstractions.Caller;
using Shared.Authorization.Abstractions.Contracts;

using Xunit;

namespace Shared.Authorization.Tests.Contracts;

public sealed class ContractValidationRulesTests
{
    private static readonly DateTime Now = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Validate_Null_ReturnsMissingContractMetadataViolation()
    {
        ContractValidationRules.Validate(null).Should().Contain("MissingContractMetadata");
    }

    [Fact]
    public void Validate_ValidMetadata_ReturnsNoViolations()
    {
        var metadata = new ContractMetadata("Safety.Editor", "corr-1", MfaStatus.NotApplicable, "acme-plant-1", Now);

        ContractValidationRules.Validate(metadata).Should().BeEmpty();
    }

    [Fact]
    public void Validate_EmptyCallerRole_ReturnsMissingCallerRoleViolation()
    {
        var metadata = new ContractMetadata("", "corr-1", MfaStatus.NotApplicable, "acme-plant-1", Now);

        ContractValidationRules.Validate(metadata).Should().Contain("MissingCallerRole");
    }

    [Fact]
    public void Validate_EmptyCorrelationId_ReturnsMissingCorrelationIdViolation()
    {
        var metadata = new ContractMetadata("Safety.Editor", "", MfaStatus.NotApplicable, "acme-plant-1", Now);

        ContractValidationRules.Validate(metadata).Should().Contain("MissingCorrelationId");
    }

    [Fact]
    public void Validate_EmptyTenantId_ReturnsMissingTenantIdViolation()
    {
        var metadata = new ContractMetadata("Safety.Editor", "corr-1", MfaStatus.NotApplicable, "", Now);

        ContractValidationRules.Validate(metadata).Should().Contain("MissingTenantId");
    }

    [Fact]
    public void Validate_DefaultTimestamp_ReturnsInvalidTimestampViolation()
    {
        var metadata = new ContractMetadata("Safety.Editor", "corr-1", MfaStatus.NotApplicable, "acme-plant-1", default);

        ContractValidationRules.Validate(metadata).Should().Contain("InvalidTimestamp");
    }
}

// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Tests.Domain.Intelligence.ModelRegistry
//  Layer:          Tests (Unit)
//  File:           ModelRegistryDomainServiceTests.cs
//  Created:        2026-08-20
//  Author:         PlantSafety Platform Team (Claude Code testing-agent)
//  SchemaVersion:  2026.1
//  Description:    Unit tests for ModelRegistryDomainService: the SAFETY-CRITICAL Safety Rule 4.5
//                  deployment-eligibility fail-safe gate (IsEligibleForProductionDeployment) and
//                  SemVer-based deployment supersession (SupersedesCurrentDeployment). Mirrors
//                  PlantSafety.Tests.Domain.Intelligence.PredictiveSafety.FailureLikelihoodPredictorTests.
//                  Isolated, deterministic, <50ms per test. No I/O, no network.
// ============================================================================
using PlantSafety.Domain.Intelligence.ModelRegistry.Aggregates;
using PlantSafety.Domain.Intelligence.ModelRegistry.Services;
using PlantSafety.Domain.Intelligence.ModelRegistry.ValueObjects;

namespace PlantSafety.Tests.Domain.Intelligence.ModelRegistry;

/// <summary>Unit tests for <see cref="ModelRegistryDomainService"/>.</summary>
public sealed class ModelRegistryDomainServiceTests : UnitTestBase
{
    private readonly ModelRegistryDomainService _sut = new();

    private static ModelRegistryEntry DraftEntry(string version = "1.0.0", double accuracy = 0.9, string modelName = "FailurePredictor") =>
        ModelRegistryEntry.Register(
            registeredBy: TestUserId,
            registeredAtUtc: FixedTimestamp,
            correlationId: TestCorrelationId,
            causationId: TestCausationId,
            actor: TestActor,
            tenantId: TestTenantId.Value,
            siteId: null,
            modelName: modelName,
            version: ModelVersion.From(version),
            datasetHash: "sha256:abc123",
            trainingAccuracyScore: accuracy);

    private static ModelRegistryEntry ApprovedEntry(string version = "1.0.0", double accuracy = 0.9, string modelName = "FailurePredictor")
    {
        var entry = DraftEntry(version, accuracy, modelName);
        entry.SubmitForApproval(TestActor, TestCorrelationId, TestCausationId);
        entry.Approve(TestUserId, TestActor, FixedTimestamp, TestCorrelationId, TestCausationId);
        return entry;
    }

    // ---- IsEligibleForProductionDeployment (Safety Rule 4.5) ---------------

    [Fact]
    public void IsEligibleForProductionDeployment_ApprovedAtExactlyThreshold_ReturnsTrue()
    {
        var entry = ApprovedEntry(accuracy: ModelRegistryDomainService.MinimumConfidenceScore);
        _sut.IsEligibleForProductionDeployment(entry).Should().BeTrue();
    }

    [Fact]
    public void IsEligibleForProductionDeployment_ApprovedAboveThreshold_ReturnsTrue()
    {
        var entry = ApprovedEntry(accuracy: 0.99);
        _sut.IsEligibleForProductionDeployment(entry).Should().BeTrue();
    }

    [Fact]
    public void IsEligibleForProductionDeployment_ApprovedBelowThreshold_ReturnsFalse_SafetyRule45FailsSafe()
    {
        var entry = ApprovedEntry(accuracy: ModelRegistryDomainService.MinimumConfidenceScore - 0.01);
        _sut.IsEligibleForProductionDeployment(entry).Should().BeFalse(
            "Safety Rule 4.5 must fail safe: an Approved model below the minimum confidence score is never deployment-eligible");
    }

    [Fact]
    public void IsEligibleForProductionDeployment_DraftStatus_ReturnsFalse()
    {
        var entry = DraftEntry(accuracy: 1.0);
        _sut.IsEligibleForProductionDeployment(entry).Should().BeFalse();
    }

    [Fact]
    public void IsEligibleForProductionDeployment_PendingApprovalStatus_ReturnsFalse()
    {
        var entry = DraftEntry(accuracy: 1.0);
        entry.SubmitForApproval(TestActor, TestCorrelationId, TestCausationId);
        _sut.IsEligibleForProductionDeployment(entry).Should().BeFalse();
    }

    [Fact]
    public void IsEligibleForProductionDeployment_DeprecatedStatus_ReturnsFalse()
    {
        var entry = ApprovedEntry(accuracy: 1.0);
        entry.Deprecate("Superseded by v2", FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        _sut.IsEligibleForProductionDeployment(entry).Should().BeFalse();
    }

    [Fact]
    public void IsEligibleForProductionDeployment_RetiredStatus_ReturnsFalse()
    {
        var entry = ApprovedEntry(accuracy: 1.0);
        entry.Retire(FixedTimestamp, TestActor, TestCorrelationId, TestCausationId);
        _sut.IsEligibleForProductionDeployment(entry).Should().BeFalse();
    }

    [Fact]
    public void IsEligibleForProductionDeployment_NullEntry_ThrowsArgumentNull()
    {
        var act = () => _sut.IsEligibleForProductionDeployment(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    // ---- SupersedesCurrentDeployment (SemVer ordinal comparison) -----------

    [Fact]
    public void SupersedesCurrentDeployment_HigherSemVerApprovedCandidate_ReturnsTrue()
    {
        var current = ApprovedEntry(version: "1.0.0");
        var candidate = ApprovedEntry(version: "1.1.0");
        _sut.SupersedesCurrentDeployment(candidate, current).Should().BeTrue();
    }

    [Fact]
    public void SupersedesCurrentDeployment_HigherMajorSemVer_ReturnsTrue()
    {
        var current = ApprovedEntry(version: "1.9.9");
        var candidate = ApprovedEntry(version: "2.0.0");
        _sut.SupersedesCurrentDeployment(candidate, current).Should().BeTrue();
    }

    [Fact]
    public void SupersedesCurrentDeployment_LowerSemVer_ReturnsFalse()
    {
        var current = ApprovedEntry(version: "1.1.0");
        var candidate = ApprovedEntry(version: "1.0.0");
        _sut.SupersedesCurrentDeployment(candidate, current).Should().BeFalse();
    }

    [Fact]
    public void SupersedesCurrentDeployment_EqualSemVer_ReturnsFalse()
    {
        var current = ApprovedEntry(version: "1.0.0");
        var candidate = ApprovedEntry(version: "1.0.0");
        _sut.SupersedesCurrentDeployment(candidate, current).Should().BeFalse();
    }

    [Fact]
    public void SupersedesCurrentDeployment_CandidateNotApproved_ReturnsFalse()
    {
        var current = ApprovedEntry(version: "1.0.0");
        var candidate = DraftEntry(version: "2.0.0");
        _sut.SupersedesCurrentDeployment(candidate, current).Should().BeFalse(
            "only a deployable (Approved) candidate can supersede the current deployment");
    }

    [Fact]
    public void SupersedesCurrentDeployment_DifferentModelName_ReturnsFalse()
    {
        var current = ApprovedEntry(version: "1.0.0", modelName: "FailurePredictor");
        var candidate = ApprovedEntry(version: "2.0.0", modelName: "AnomalyDetector");
        _sut.SupersedesCurrentDeployment(candidate, current).Should().BeFalse();
    }

    [Fact]
    public void SupersedesCurrentDeployment_ModelNameCaseInsensitive_StillMatches()
    {
        var current = ApprovedEntry(version: "1.0.0", modelName: "FailurePredictor");
        var candidate = ApprovedEntry(version: "2.0.0", modelName: "failurepredictor");
        _sut.SupersedesCurrentDeployment(candidate, current).Should().BeTrue();
    }

    [Fact]
    public void SupersedesCurrentDeployment_NullCandidate_ThrowsArgumentNull()
    {
        var current = ApprovedEntry();
        var act = () => _sut.SupersedesCurrentDeployment(null!, current);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void SupersedesCurrentDeployment_NullCurrent_ThrowsArgumentNull()
    {
        var candidate = ApprovedEntry();
        var act = () => _sut.SupersedesCurrentDeployment(candidate, null!);
        act.Should().Throw<ArgumentNullException>();
    }
}

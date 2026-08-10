// ============================================================================
//  Project:        Shared.PredictiveSafety.Tests
//  File:           PredictiveSafetyDtoMapperTests.cs
//  Created:        2026-08-10
// ============================================================================
using FluentAssertions;

using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.Regulatory;
using Shared.FacilityDomain.Abstractions.Severity;
using Shared.PredictiveSafety.Abstractions.Audit;
using Shared.PredictiveSafety.Abstractions.Common;
using Shared.PredictiveSafety.Abstractions.Contracts;
using Shared.PredictiveSafety.Abstractions.Hazard;
using Shared.PredictiveSafety.Abstractions.Maintenance;
using Shared.PredictiveSafety.Abstractions.Routing;
using Shared.PredictiveSafety.Abstractions.Scheduling;

using Xunit;

namespace Shared.PredictiveSafety.Tests.Contracts;

public sealed class PredictiveSafetyDtoMapperTests
{
    [Fact]
    public void ToDto_ProjectsEveryDomainField()
    {
        var metadata = new PredictiveSafetyContractMetadata(PredictiveSafetyContractMetadata.CurrentSchemaVersion, "acme-plant-1", "corr-1", DateTime.UtcNow);
        var hazard = new PredictiveHazardDecision(HazardType.Fire, SeverityLevel.Severe, Array.Empty<RegulatoryJurisdiction>(), true, PredictivePriority.High, new[] { "R1" });
        var maintenance = new PredictiveMaintenanceDecision(Guid.NewGuid(), true, PredictivePriority.Medium, new[] { "R2" });
        var routing = new PredictiveRoutingDecision("ChemicalSpill", Guid.NewGuid(), false, PredictivePriority.Low, Array.Empty<string>());
        var scheduling = new PredictiveSchedulingDecision(true, PredictivePriority.High, new[] { "R3" });
        var audit = new PredictiveAuditAnomaly(false, PredictivePriority.Low, Array.Empty<string>());
        var result = new PredictiveSafetyEvaluationResultContract(metadata, hazard, maintenance, routing, scheduling, audit, PredictivePriority.High);

        PredictiveSafetyEvaluationDto dto = PredictiveSafetyDtoMapper.ToDto(result);

        dto.HazardType.Should().Be("Fire");
        dto.HazardRequiresEscalation.Should().BeTrue();
        dto.MaintenanceRequiresImmediate.Should().BeTrue();
        dto.RoutingRequiresPriority.Should().BeFalse();
        dto.SchedulingRequiresAcceleration.Should().BeTrue();
        dto.AuditAnomalyDetected.Should().BeFalse();
        dto.OverallPriority.Should().Be("High");
    }

    [Fact]
    public void ToDto_NullResult_Throws()
    {
        Action act = () => PredictiveSafetyDtoMapper.ToDto(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}

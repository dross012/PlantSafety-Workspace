// ============================================================================
//  Project:        PlantSafety.UI.Api
//  Module:         Controllers
//  File:           DigitalTwinController.cs
//  Created:        2026-08-09
//  Description:    "Run this engine" surface over Enterprise.DigitalTwin's
//                  five pure deterministic "what if" simulations (Phase D.7).
// ============================================================================
using Microsoft.AspNetCore.Mvc;

using Shared.FacilityDomain.Abstractions.Chains;
using Shared.FacilityDomain.Abstractions.Hazards;
using Shared.FacilityDomain.Abstractions.Lifecycle;
using Shared.FacilityDomain.Abstractions.Propagation;
using Shared.FacilityDomain.Abstractions.Severity;

using Shared.WorkflowEngine.Abstractions.Engine;

using Enterprise.DigitalTwin.Abstractions.Simulation;

using Shared.Deterministic.Abstractions.Clock;

namespace PlantSafety.UI.Api.Controllers;

/// <summary>Request body for the severity-progression simulation.</summary>
public sealed record SeverityProgressionRequest(HazardType HazardType, int EmployeeInjuryCount, int MachineFailureCount, int EventChainLength);

/// <summary>One (stage, severity) pair for the multi-hazard-chain simulation.</summary>
public sealed record ChainStageRequest(PropagationStage Stage, SeverityLevel Severity);

/// <summary>Request body for the multi-hazard-chain simulation.</summary>
public sealed record MultiHazardChainRequest(IReadOnlyList<ChainStageRequest> Stages);

/// <summary>Request body for the facility-lifecycle simulation.</summary>
public sealed record FacilityLifecycleSimulationRequest(FacilityLifecycleStatus CurrentStatus, FacilityLifecycleAction Action);

/// <summary>Runs Enterprise.DigitalTwin's five deterministic "what if" simulations.</summary>
[ApiController]
[Route("api/v1/ui/engines/digital-twin")]
public sealed class DigitalTwinController(IClock clock) : ControllerBase
{
    /// <summary>POST .../hazard-propagation.</summary>
    [HttpPost("hazard-propagation")]
    public ActionResult<IReadOnlyList<PropagationStage>> HazardPropagation([FromBody] PropagationStage originStage) =>
        Ok(DigitalTwinSimulationEngine.SimulateHazardPropagation(originStage));

    /// <summary>POST .../severity-progression.</summary>
    [HttpPost("severity-progression")]
    public ActionResult<SeverityLevel> SeverityProgression([FromBody] SeverityProgressionRequest request) =>
        Ok(DigitalTwinSimulationEngine.SimulateSeverityProgression(request.HazardType, request.EmployeeInjuryCount, request.MachineFailureCount, request.EventChainLength));

    /// <summary>POST .../workflow.</summary>
    [HttpPost("workflow")]
    public ActionResult<WorkflowDecision> Workflow([FromBody] WorkflowContext context) =>
        Ok(DigitalTwinSimulationEngine.SimulateWorkflow(context));

    /// <summary>POST .../multi-hazard-chain.</summary>
    [HttpPost("multi-hazard-chain")]
    public ActionResult<HazardChain> MultiHazardChain([FromBody] MultiHazardChainRequest request) =>
        Ok(DigitalTwinSimulationEngine.SimulateMultiHazardChain(
            request.Stages.Select(s => (s.Stage, s.Severity)).ToArray(), clock.UtcNow));

    /// <summary>POST .../facility-lifecycle -- returns 400 if the transition is invalid.</summary>
    [HttpPost("facility-lifecycle")]
    public ActionResult<FacilityLifecycleStatus> FacilityLifecycle([FromBody] FacilityLifecycleSimulationRequest request)
    {
        try
        {
            return Ok(DigitalTwinSimulationEngine.SimulateFacilityLifecycle(request.CurrentStatus, request.Action));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

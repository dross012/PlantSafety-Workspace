// ============================================================================
//  Project:        Enterprise.DigitalTwin
//  Module:         Abstractions.Simulation
//  File:           FacilityLifecycleAction.cs
//  Created:        2026-08-08
//  Description:    Phase D.7: the four lifecycle actions
//                  DigitalTwinSimulationEngine.SimulateFacilityLifecycle
//                  can simulate, mirroring FacilityAggregate's Phase C.12
//                  commands (ActivateFacility/ShutdownFacility/
//                  ReopenFacility/DecommissionFacility) one-to-one.
// ============================================================================
namespace Enterprise.DigitalTwin.Abstractions.Simulation;

/// <summary>A lifecycle action to simulate against a <see cref="Twins.DigitalTwinFacility"/>.</summary>
public enum FacilityLifecycleAction
{
    /// <summary>Mirrors FacilityAggregate.ActivateFacility.</summary>
    Activate,

    /// <summary>Mirrors FacilityAggregate.ShutdownFacility.</summary>
    Shutdown,

    /// <summary>Mirrors FacilityAggregate.ReopenFacility.</summary>
    Reopen,

    /// <summary>Mirrors FacilityAggregate.DecommissionFacility.</summary>
    Decommission,
}

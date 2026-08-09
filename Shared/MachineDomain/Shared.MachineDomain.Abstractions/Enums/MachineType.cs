// ============================================================================
//  Project:        Shared.MachineDomain
//  Module:         Abstractions.Enums
//  File:           MachineType.cs
//  Created:        2026-08-08
//  Description:    Phase C.2: the equipment-type vocabulary for a machine.
//                  Additive-only: new members may be appended, existing
//                  members must never be renumbered or removed once
//                  published.
// ============================================================================
namespace Shared.MachineDomain.Abstractions.Enums;

/// <summary>The type of a piece of machinery/equipment.</summary>
public enum MachineType
{
    /// <summary>Material-handling conveyor system.</summary>
    ConveyorSystem,

    /// <summary>A vessel operating under pressure.</summary>
    PressureVessel,

    /// <summary>A boiler or steam-generating unit.</summary>
    Boiler,

    /// <summary>A powered industrial truck (forklift).</summary>
    Forklift,

    /// <summary>An overhead or mobile crane.</summary>
    Crane,

    /// <summary>Robotic/automated production equipment.</summary>
    Robotics,

    /// <summary>Heating, ventilation, and air conditioning equipment.</summary>
    HVAC,

    /// <summary>Electrical distribution/switching equipment.</summary>
    ElectricalPanel,

    /// <summary>A machine type that does not match any of the above -- the classification default, never a data-entry error.</summary>
    Other,
}

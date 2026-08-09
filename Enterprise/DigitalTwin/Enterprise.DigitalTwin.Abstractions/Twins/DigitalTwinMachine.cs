// ============================================================================
//  Project:        Enterprise.DigitalTwin
//  Module:         Abstractions.Twins
//  File:           DigitalTwinMachine.cs
//  Created:        2026-08-08
//  Description:    Phase D.7: a read-only virtual snapshot of a real
//                  Shared.MachineDomain machine, for simulation purposes
//                  only.
// ============================================================================
namespace Enterprise.DigitalTwin.Abstractions.Twins;

/// <summary>A read-only virtual snapshot of a real Shared.MachineDomain machine.</summary>
public sealed record DigitalTwinMachine(
    Guid MachineId,
    string Name,
    int FailureCount);

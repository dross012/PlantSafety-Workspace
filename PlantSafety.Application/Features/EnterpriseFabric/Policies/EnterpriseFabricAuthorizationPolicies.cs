// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Authorization policy names for the EnterpriseFabric module.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.EnterpriseFabric.Policies;

/// <summary>Authorization policy name constants for EnterpriseFabric features.</summary>
public static class EnterpriseFabricAuthorizationPolicies
{
    /// <summary>Policy that grants access to create user accounts.</summary>
    public const string CanCreateUser = "EnterpriseFabric.CanCreateUser";

    /// <summary>Policy that grants access to update user accounts.</summary>
    public const string CanUpdateUser = "EnterpriseFabric.CanUpdateUser";

    /// <summary>Policy that grants access to deactivate user accounts.</summary>
    public const string CanDeactivateUser = "EnterpriseFabric.CanDeactivateUser";

    /// <summary>Policy that grants access to assign and revoke roles.</summary>
    public const string CanAssignRole = "EnterpriseFabric.CanAssignRole";

    /// <summary>Policy that grants access to manage AI governance settings.</summary>
    public const string CanManageAIGovernance = "EnterpriseFabric.CanManageAIGovernance";

    /// <summary>Policy that grants access to view Digital Twin data.</summary>
    public const string CanViewDigitalTwin = "EnterpriseFabric.CanViewDigitalTwin";

    /// <summary>Policy that grants access to the data lake.</summary>
    public const string CanAccessDataLake = "EnterpriseFabric.CanAccessDataLake";
}

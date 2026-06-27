// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Permission constants for the EnterpriseFabric module.
// SchemaVersion: 1

namespace PlantSafety.Application.Features.EnterpriseFabric.Permissions;

/// <summary>Permission strings for the EnterpriseFabric module.  Format: <c>Module.Feature.Action</c>.</summary>
public static class EnterpriseFabricPermissions
{
    /// <summary>User management permissions.</summary>
    public static class Users
    {
        /// <summary>Create a user account.</summary>
        public const string Create = "EnterpriseFabric.Users.Create";
        /// <summary>Update a user account.</summary>
        public const string Update = "EnterpriseFabric.Users.Update";
        /// <summary>Deactivate a user account.</summary>
        public const string Deactivate = "EnterpriseFabric.Users.Deactivate";
        /// <summary>View a user profile.</summary>
        public const string View = "EnterpriseFabric.Users.View";
        /// <summary>List users.</summary>
        public const string List = "EnterpriseFabric.Users.List";
        /// <summary>Assign or revoke roles.</summary>
        public const string AssignRole = "EnterpriseFabric.Users.AssignRole";
    }

    /// <summary>AI governance permissions.</summary>
    public static class AIGovernance
    {
        /// <summary>Manage AI governance policies.</summary>
        public const string Manage = "EnterpriseFabric.AIGovernance.Manage";
        /// <summary>View AI governance configuration.</summary>
        public const string View = "EnterpriseFabric.AIGovernance.View";
    }

    /// <summary>Digital twin permissions.</summary>
    public static class DigitalTwin
    {
        /// <summary>View digital twin data.</summary>
        public const string View = "EnterpriseFabric.DigitalTwin.View";
    }

    /// <summary>Data lake permissions.</summary>
    public static class DataLake
    {
        /// <summary>Access data lake records.</summary>
        public const string Access = "EnterpriseFabric.DataLake.Access";
    }

    /// <summary>Sustainability and environmental health permissions.</summary>
    public static class Sustainability
    {
        /// <summary>View sustainability metrics.</summary>
        public const string View = "EnterpriseFabric.Sustainability.View";
    }
}

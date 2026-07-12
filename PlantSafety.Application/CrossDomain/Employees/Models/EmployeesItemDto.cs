// AUTHOR: PlantSafety Platform Team
// CREATED: 2026-06-17
// FILE DESCRIPTION: Read-model item DTO for a single Employee record.
// SchemaVersion: 1

namespace PlantSafety.Application.CrossDomain.Employees.Models;

/// <summary>A single employee record.</summary>
public sealed record EmployeesItemDto
{
    /// <summary>Employee identifier.</summary>
    public required Guid EmployeeId { get; init; }

    /// <summary>Tenant identifier.</summary>
    public required Guid TenantId { get; init; }

    /// <summary>Site identifier, or null when not site-scoped.</summary>
    public Guid? SiteId { get; init; }

    /// <summary>Employee full name.</summary>
    public required string FullName { get; init; }

    /// <summary>Employee number/badge identifier.</summary>
    public required string EmployeeNumber { get; init; }

    /// <summary>Department the employee belongs to.</summary>
    public required string Department { get; init; }

    /// <summary>Current employment status (e.g., Active/OnLeave/Terminated).</summary>
    public required string Status { get; init; }

    /// <summary>UTC date the employee was hired.</summary>
    public required DateTime HireDateUtc { get; init; }

    /// <summary>Identity of the employee's assigned manager.</summary>
    public required Guid ManagerId { get; init; }
}

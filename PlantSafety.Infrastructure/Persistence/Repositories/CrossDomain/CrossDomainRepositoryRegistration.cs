// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         Persistence.Repositories.CrossDomain
//  Layer:          Infrastructure
//  File:           CrossDomainRepositoryRegistration.cs
//  Created:        2026-06-17
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:    DI registration helper for all 12 CrossDomain repositories
//                  (Administration/Backup/Cleanup/Employees/Machines/Operators/
//                  Permissions/Restore/Roles/Updates/Users/WorkCenters). Every
//                  repository class listed below already fully implements its
//                  interface and calls stored procedures deployed under the
//                  [CrossDomain] SQL schema. NOT invoked by the shared
//                  InfrastructureServiceRegistration.AddInfrastructure() yet --
//                  wiring the call site there is a required, tracked follow-up
//                  for a human maintainer (per task constraints, that central
//                  file is intentionally not edited here to avoid concurrent-edit
//                  collisions with other agents). Additive-only evolution.
// ============================================================================
using PlantSafety.Application.CrossDomain.Administration.Repositories;
using PlantSafety.Application.CrossDomain.Backup.Repositories;
using PlantSafety.Application.CrossDomain.Cleanup.Repositories;
using PlantSafety.Application.CrossDomain.Employees.Repositories;
using PlantSafety.Application.CrossDomain.Machines.Repositories;
using PlantSafety.Application.CrossDomain.Operators.Repositories;
using PlantSafety.Application.CrossDomain.Permissions.Repositories;
using PlantSafety.Application.CrossDomain.Restore.Repositories;
using PlantSafety.Application.CrossDomain.Roles.Repositories;
using PlantSafety.Application.CrossDomain.Updates.Repositories;
using PlantSafety.Application.CrossDomain.Users.Repositories;
using PlantSafety.Application.CrossDomain.WorkCenters.Repositories;
using PlantSafety.Infrastructure.CrossDomain.Administration.Persistence;
using PlantSafety.Infrastructure.CrossDomain.Backup.Persistence;
using PlantSafety.Infrastructure.CrossDomain.Cleanup.Persistence;
using PlantSafety.Infrastructure.CrossDomain.Employees.Persistence;
using PlantSafety.Infrastructure.CrossDomain.Machines.Persistence;
using PlantSafety.Infrastructure.CrossDomain.Operators.Persistence;
using PlantSafety.Infrastructure.CrossDomain.Permissions.Persistence;
using PlantSafety.Infrastructure.CrossDomain.Restore.Persistence;
using PlantSafety.Infrastructure.CrossDomain.Roles.Persistence;
using PlantSafety.Infrastructure.CrossDomain.Updates.Persistence;
using PlantSafety.Infrastructure.CrossDomain.Users.Persistence;
using PlantSafety.Infrastructure.CrossDomain.WorkCenters.Persistence;

namespace PlantSafety.Infrastructure.Persistence.Repositories.CrossDomain;

/// <summary>
/// Extension method that registers all <c>CrossDomain</c> repositories into the DI container.
/// Called from <c>InfrastructureServiceRegistration.AddInfrastructure</c>.
/// </summary>
public static class CrossDomainRepositoryRegistration
{
    /// <summary>Registers all 12 CrossDomain Dapper repositories.</summary>
    public static IServiceCollection AddCrossDomainRepositories(this IServiceCollection services)
    {
        services.AddScoped<IAdministrationRepository, AdministrationDapperRepository>();
        services.AddScoped<IBackupRepository, BackupDapperRepository>();
        services.AddScoped<ICleanupRepository, CleanupDapperRepository>();
        services.AddScoped<IEmployeesRepository, EmployeesDapperRepository>();
        services.AddScoped<IMachinesRepository, MachinesDapperRepository>();
        services.AddScoped<IOperatorsRepository, OperatorsDapperRepository>();
        services.AddScoped<IPermissionsRepository, PermissionsDapperRepository>();
        services.AddScoped<IRestoreRepository, RestoreDapperRepository>();
        services.AddScoped<IRolesRepository, RolesDapperRepository>();
        services.AddScoped<IUpdatesRepository, UpdatesDapperRepository>();
        services.AddScoped<IUsersRepository, UsersDapperRepository>();
        services.AddScoped<IWorkCentersRepository, WorkCentersDapperRepository>();

        return services;
    }
}

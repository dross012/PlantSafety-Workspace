-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         CrossDomain.Employees
--  Layer:          Infrastructure - SQL Scripts
--  File:           Employees.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the Employees read-model, backing
--                  IEmployeesRepository.GetEmployeesAsync.
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Employees' AND schema_id = SCHEMA_ID('CrossDomain'))
BEGIN
    CREATE TABLE [CrossDomain].[Employees] (
        EmployeeId       UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        TenantId          UNIQUEIDENTIFIER NOT NULL,
        SiteId             UNIQUEIDENTIFIER NULL,
        FullName          NVARCHAR(200)    NOT NULL,
        EmployeeNumber    NVARCHAR(50)     NOT NULL,
        Department         NVARCHAR(100)    NOT NULL,
        Status              NVARCHAR(50)     NOT NULL,
        HireDateUtc        DATETIME2(3)     NOT NULL,
        ManagerId          UNIQUEIDENTIFIER NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_Employees_TenantId_SiteId ON [CrossDomain].[Employees] (TenantId, SiteId);
    CREATE NONCLUSTERED INDEX IX_Employees_HireDateUtc ON [CrossDomain].[Employees] (HireDateUtc);
END
GO

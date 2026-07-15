-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         CrossDomain.Employees
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_Employees_GetEmployees.sql
--  Created:        2026-07-13
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Read-only list procedure for [CrossDomain].[Employees],
--                  backing IEmployeesRepository.GetEmployeesAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [CrossDomain].[usp_Employees_GetEmployees]
    @TenantId   UNIQUEIDENTIFIER,
    @SiteId     UNIQUEIDENTIFIER = NULL,
    @FromUtc    DATETIME2(3),
    @ToUtc      DATETIME2(3)
AS
SET NOCOUNT ON;

SELECT
        [EmployeeId],
        [TenantId],
        [SiteId],
        [FullName],
        [EmployeeNumber],
        [Department],
        [Status],
        [HireDateUtc],
        [ManagerId]
FROM [CrossDomain].[Employees]
WHERE TenantId = @TenantId
  AND (@SiteId IS NULL OR SiteId = @SiteId)
  AND [HireDateUtc] BETWEEN @FromUtc AND @ToUtc
ORDER BY [HireDateUtc] DESC;
GO

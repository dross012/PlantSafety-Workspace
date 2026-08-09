-- ============================================================================
--  Project:        Shared.EmployeeDomain -- Phase1-Employee
--  File:           10-indexes.sql
--  Created:        2026-08-09
--  Description:    Lookups by role and time-range queries on the
--                  append-only exposure/injury/certification history.
--  Idempotent: guarded by sys.indexes existence checks.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Employee_Role' AND object_id = OBJECT_ID('EmployeeDomain.Employee'))
    CREATE NONCLUSTERED INDEX IX_Employee_Role ON [EmployeeDomain].[Employee] ([Role]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EmployeeExposureRecord_EmployeeId_ExposedAtUtc' AND object_id = OBJECT_ID('EmployeeDomain.EmployeeExposureRecord'))
    CREATE NONCLUSTERED INDEX IX_EmployeeExposureRecord_EmployeeId_ExposedAtUtc
        ON [EmployeeDomain].[EmployeeExposureRecord] ([EmployeeId], [ExposedAtUtc] DESC);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EmployeeExposureRecord_HazardTypeCode' AND object_id = OBJECT_ID('EmployeeDomain.EmployeeExposureRecord'))
    CREATE NONCLUSTERED INDEX IX_EmployeeExposureRecord_HazardTypeCode
        ON [EmployeeDomain].[EmployeeExposureRecord] ([HazardTypeCode]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EmployeeInjuryEvent_EmployeeId_OccurredAtUtc' AND object_id = OBJECT_ID('EmployeeDomain.EmployeeInjuryEvent'))
    CREATE NONCLUSTERED INDEX IX_EmployeeInjuryEvent_EmployeeId_OccurredAtUtc
        ON [EmployeeDomain].[EmployeeInjuryEvent] ([EmployeeId], [OccurredAtUtc] DESC);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EmployeeCertification_EmployeeId_ExpiresAtUtc' AND object_id = OBJECT_ID('EmployeeDomain.EmployeeCertification'))
    CREATE NONCLUSTERED INDEX IX_EmployeeCertification_EmployeeId_ExpiresAtUtc
        ON [EmployeeDomain].[EmployeeCertification] ([EmployeeId], [ExpiresAtUtc]);
GO

-- ============================================================================
--  Project:        Shared.MachineDomain -- Phase1-Machine
--  File:           10-indexes.sql
--  Created:        2026-08-09
--  Idempotent: guarded by sys.indexes existence checks.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Machine_FacilityId' AND object_id = OBJECT_ID('MachineDomain.Machine'))
    CREATE NONCLUSTERED INDEX IX_Machine_FacilityId ON [MachineDomain].[Machine] ([FacilityId]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Machine_NextScheduledMaintenanceAtUtc' AND object_id = OBJECT_ID('MachineDomain.Machine'))
    CREATE NONCLUSTERED INDEX IX_Machine_NextScheduledMaintenanceAtUtc ON [MachineDomain].[Machine] ([NextScheduledMaintenanceAtUtc]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_MachineFailureRecord_MachineId_FailedAtUtc' AND object_id = OBJECT_ID('MachineDomain.MachineFailureRecord'))
    CREATE NONCLUSTERED INDEX IX_MachineFailureRecord_MachineId_FailedAtUtc
        ON [MachineDomain].[MachineFailureRecord] ([MachineId], [FailedAtUtc] DESC);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_MachineFailureRecord_ResultingHazardTypeCode' AND object_id = OBJECT_ID('MachineDomain.MachineFailureRecord'))
    CREATE NONCLUSTERED INDEX IX_MachineFailureRecord_ResultingHazardTypeCode
        ON [MachineDomain].[MachineFailureRecord] ([ResultingHazardTypeCode]);
GO

-- ============================================================================
--  Project:        Enterprise.DigitalTwin -- Phase1-DigitalTwin
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

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TwinSnapshotLog_SimulatedAtUtc' AND object_id = OBJECT_ID('DigitalTwin.TwinSnapshotLog'))
    CREATE NONCLUSTERED INDEX IX_TwinSnapshotLog_SimulatedAtUtc ON [DigitalTwin].[TwinSnapshotLog] ([SimulatedAtUtc] DESC);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TwinSnapshotLog_SimulatedAction' AND object_id = OBJECT_ID('DigitalTwin.TwinSnapshotLog'))
    CREATE NONCLUSTERED INDEX IX_TwinSnapshotLog_SimulatedAction ON [DigitalTwin].[TwinSnapshotLog] ([SimulatedAction]);
GO

-- ============================================================================
--  Project:        Enterprise.DigitalTwin -- Phase1-DigitalTwin
--  File:           01-table-twinsnapshotlog.sql
--  Created:        2026-08-09
--  Description:    Table DDL for [DigitalTwin].[TwinSnapshotLog] -- one row
--                  per real DigitalTwinSimulationEngine run. FacilityJson/
--                  EmployeeJson/MachineJson/HazardJson/WorkflowJson/
--                  EventJson each hold the serialized shape of
--                  DigitalTwinFacility/Employee/Machine/Hazard/Workflow/
--                  Event.cs respectively (all nullable -- a given simulation
--                  run may snapshot only a subset of twin kinds).
--                  SimulatedAction (FacilityLifecycleAction, per
--                  Simulation/FacilityLifecycleAction.cs) captures which
--                  "what if" action the run evaluated.
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TwinSnapshotLog' AND schema_id = SCHEMA_ID('DigitalTwin'))
BEGIN
    CREATE TABLE [DigitalTwin].[TwinSnapshotLog]
    (
        [TwinSnapshotLogId]  UNIQUEIDENTIFIER  NOT NULL CONSTRAINT DF_TwinSnapshotLog_Id DEFAULT NEWSEQUENTIALID(),
        [SimulatedAction]     NVARCHAR(50)      NULL,
        [FacilityJson]        NVARCHAR(MAX)     NULL,
        [EmployeeJson]        NVARCHAR(MAX)     NULL,
        [MachineJson]         NVARCHAR(MAX)     NULL,
        [HazardJson]          NVARCHAR(MAX)     NULL,
        [WorkflowJson]        NVARCHAR(MAX)     NULL,
        [EventJson]           NVARCHAR(MAX)     NULL,
        [SimulatedAtUtc]      DATETIME2(3)      NOT NULL,

        [CreatedUtc]          DATETIME2(3)      NOT NULL CONSTRAINT DF_TwinSnapshotLog_CreatedUtc DEFAULT SYSUTCDATETIME(),
        [CreatedBy]           NVARCHAR(128)     NOT NULL CONSTRAINT DF_TwinSnapshotLog_CreatedBy DEFAULT SYSTEM_USER,
        [IsDeleted]           BIT               NOT NULL CONSTRAINT DF_TwinSnapshotLog_IsDeleted DEFAULT (0),

        CONSTRAINT PK_TwinSnapshotLog PRIMARY KEY CLUSTERED ([TwinSnapshotLogId]),
        CONSTRAINT CK_TwinSnapshotLog_FacilityJson CHECK ([FacilityJson] IS NULL OR ISJSON([FacilityJson]) = 1),
        CONSTRAINT CK_TwinSnapshotLog_EmployeeJson CHECK ([EmployeeJson] IS NULL OR ISJSON([EmployeeJson]) = 1),
        CONSTRAINT CK_TwinSnapshotLog_MachineJson CHECK ([MachineJson] IS NULL OR ISJSON([MachineJson]) = 1),
        CONSTRAINT CK_TwinSnapshotLog_HazardJson CHECK ([HazardJson] IS NULL OR ISJSON([HazardJson]) = 1),
        CONSTRAINT CK_TwinSnapshotLog_WorkflowJson CHECK ([WorkflowJson] IS NULL OR ISJSON([WorkflowJson]) = 1),
        CONSTRAINT CK_TwinSnapshotLog_EventJson CHECK ([EventJson] IS NULL OR ISJSON([EventJson]) = 1)
    );
END
GO

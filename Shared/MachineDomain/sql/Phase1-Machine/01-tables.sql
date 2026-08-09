-- ============================================================================
--  Project:        Shared.MachineDomain -- Phase1-Machine
--  File:           01-tables.sql
--  Created:        2026-08-09
--  Description:    Table DDL mirroring MachineState.cs exactly:
--                  [MachineDomain].[Machine] (Name/MachineType/
--                  Location.FacilityId+AreaDescription/MaintenanceProfile
--                  scalars/SafetyScore/timestamps) plus
--                  [MachineDomain].[MachineFailureRecord] (append-only
--                  failure history, ResultingHazardType FK'd to the
--                  canonical [Lookup].[HazardType] table). Location.
--                  FacilityId FKs into Shared.FacilityDomain's real
--                  Facility table (facility-aware by construction, per
--                  MachineLocation.cs's own header).
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Machine' AND schema_id = SCHEMA_ID('MachineDomain'))
BEGIN
    CREATE TABLE [MachineDomain].[Machine]
    (
        [MachineId]                      UNIQUEIDENTIFIER  NOT NULL,
        [Name]                            NVARCHAR(200)     NOT NULL,
        [MachineType]                     NVARCHAR(30)      NOT NULL,
        [FacilityId]                      UNIQUEIDENTIFIER  NOT NULL,
        [AreaDescription]                 NVARCHAR(400)     NOT NULL,
        [LastMaintenanceAtUtc]            DATETIME2(3)      NULL,
        [NextScheduledMaintenanceAtUtc]   DATETIME2(3)      NULL,
        [MaintenanceIntervalDays]         INT               NOT NULL,
        [MaintenanceProfileUpdatedAtUtc]  DATETIME2(3)      NOT NULL,
        [SafetyScoreValue]                FLOAT             NOT NULL,
        [SafetyScoreComputedAtUtc]        DATETIME2(3)      NOT NULL,
        [CreatedAtUtc]                    DATETIME2(3)      NOT NULL,
        [UpdatedAtUtc]                    DATETIME2(3)      NOT NULL,

        [CreatedUtc]                      DATETIME2(3)      NOT NULL CONSTRAINT DF_Machine_CreatedUtc DEFAULT SYSUTCDATETIME(),
        [CreatedBy]                       NVARCHAR(128)     NOT NULL CONSTRAINT DF_Machine_CreatedBy DEFAULT SYSTEM_USER,
        [UpdatedUtc]                      DATETIME2(3)      NULL,
        [UpdatedBy]                       NVARCHAR(128)     NULL,
        [IsDeleted]                       BIT               NOT NULL CONSTRAINT DF_Machine_IsDeleted DEFAULT (0),

        CONSTRAINT PK_Machine PRIMARY KEY CLUSTERED ([MachineId]),
        CONSTRAINT FK_Machine_Facility FOREIGN KEY ([FacilityId])
            REFERENCES [FacilityDomain].[Facility] ([FacilityId]),
        CONSTRAINT CK_Machine_MachineType CHECK ([MachineType] IN
            (N'ConveyorSystem', N'PressureVessel', N'Boiler', N'Forklift', N'Crane', N'Robotics', N'HVAC', N'ElectricalPanel', N'Other')),
        CONSTRAINT CK_Machine_SafetyScoreValue CHECK ([SafetyScoreValue] BETWEEN 0 AND 100)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'MachineFailureRecord' AND schema_id = SCHEMA_ID('MachineDomain'))
BEGIN
    CREATE TABLE [MachineDomain].[MachineFailureRecord]
    (
        [MachineFailureRecordId]  UNIQUEIDENTIFIER  NOT NULL CONSTRAINT DF_MachineFailureRecord_Id DEFAULT NEWSEQUENTIALID(),
        [MachineId]                UNIQUEIDENTIFIER  NOT NULL,
        [Description]              NVARCHAR(500)     NOT NULL,
        [ResultingHazardTypeCode]  NVARCHAR(30)      NOT NULL,
        [FailedAtUtc]              DATETIME2(3)      NOT NULL,

        [CreatedUtc]               DATETIME2(3)      NOT NULL CONSTRAINT DF_MachineFailureRecord_CreatedUtc DEFAULT SYSUTCDATETIME(),
        [CreatedBy]                NVARCHAR(128)     NOT NULL CONSTRAINT DF_MachineFailureRecord_CreatedBy DEFAULT SYSTEM_USER,
        [IsDeleted]                BIT               NOT NULL CONSTRAINT DF_MachineFailureRecord_IsDeleted DEFAULT (0),

        CONSTRAINT PK_MachineFailureRecord PRIMARY KEY CLUSTERED ([MachineFailureRecordId]),
        CONSTRAINT FK_MachineFailureRecord_Machine FOREIGN KEY ([MachineId])
            REFERENCES [MachineDomain].[Machine] ([MachineId]),
        CONSTRAINT FK_MachineFailureRecord_HazardType FOREIGN KEY ([ResultingHazardTypeCode])
            REFERENCES [Lookup].[HazardType] ([Code])
    );
END
GO

-- ============================================================================
--  Project:        Shared.EmployeeDomain -- Phase1-Employee
--  File:           01-tables.sql
--  Created:        2026-08-09
--  Description:    Table DDL mirroring EmployeeState.cs exactly:
--                  [EmployeeDomain].[Employee] (scalar identity: Name, Role,
--                  TrainingProfile.UpdatedAtUtc, SafetyScore.ScoreValue/
--                  ComputedAtUtc, CreatedAtUtc/UpdatedAtUtc), plus four
--                  child tables for the four IReadOnlyList<T>/collection
--                  members: EmployeeTrainingCode (TrainingProfile.
--                  CompletedTrainingCodes, replace-wholesale),
--                  EmployeeCertification (issued certs, append/expire in
--                  place -- not replace-wholesale, since Certifications is a
--                  real issued-certificate ledger), EmployeeExposureRecord
--                  (append-only hazard exposure history, HazardType FK'd to
--                  the canonical [Lookup] table), and injury history is
--                  flattened onto Employee itself (EmployeeInjuryProfile has
--                  no per-injury list in the C# type -- only a running
--                  TotalInjuryCount/LastInjuryAtUtc/LastInjuryDescription
--                  summary) plus an EmployeeInjuryEvent append-only log for
--                  full auditability of RecordInjury calls.
--  Idempotent: safe to re-run. Additive-only evolution.
-- ============================================================================

USE [PlantSafety.CoreDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Employee' AND schema_id = SCHEMA_ID('EmployeeDomain'))
BEGIN
    CREATE TABLE [EmployeeDomain].[Employee]
    (
        [EmployeeId]                    UNIQUEIDENTIFIER  NOT NULL,
        [Name]                          NVARCHAR(200)     NOT NULL,
        [Role]                          NVARCHAR(30)      NOT NULL,
        [TrainingProfileUpdatedAtUtc]   DATETIME2(3)      NOT NULL,
        [InjuryTotalCount]              INT               NOT NULL CONSTRAINT DF_Employee_InjuryTotalCount DEFAULT (0),
        [InjuryLastAtUtc]               DATETIME2(3)      NULL,
        [InjuryLastDescription]         NVARCHAR(500)     NULL,
        [InjuryProfileUpdatedAtUtc]     DATETIME2(3)      NOT NULL,
        [SafetyScoreValue]              FLOAT             NOT NULL,
        [SafetyScoreComputedAtUtc]      DATETIME2(3)      NOT NULL,
        [CreatedAtUtc]                  DATETIME2(3)      NOT NULL,
        [UpdatedAtUtc]                  DATETIME2(3)      NOT NULL,

        [CreatedUtc]                    DATETIME2(3)      NOT NULL CONSTRAINT DF_Employee_CreatedUtc DEFAULT SYSUTCDATETIME(),
        [CreatedBy]                     NVARCHAR(128)     NOT NULL CONSTRAINT DF_Employee_CreatedBy DEFAULT SYSTEM_USER,
        [UpdatedUtc]                    DATETIME2(3)      NULL,
        [UpdatedBy]                     NVARCHAR(128)     NULL,
        [IsDeleted]                     BIT               NOT NULL CONSTRAINT DF_Employee_IsDeleted DEFAULT (0),

        CONSTRAINT PK_Employee PRIMARY KEY CLUSTERED ([EmployeeId]),
        CONSTRAINT CK_Employee_Role CHECK ([Role] IN
            (N'Operator', N'Supervisor', N'Technician', N'Maintenance', N'Safety', N'Contractor', N'Administrator', N'Other')),
        CONSTRAINT CK_Employee_SafetyScoreValue CHECK ([SafetyScoreValue] BETWEEN 0 AND 100)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EmployeeTrainingCode' AND schema_id = SCHEMA_ID('EmployeeDomain'))
BEGIN
    CREATE TABLE [EmployeeDomain].[EmployeeTrainingCode]
    (
        [EmployeeId]      UNIQUEIDENTIFIER  NOT NULL,
        [TrainingCode]    NVARCHAR(50)      NOT NULL,
        [UpdatedAtUtc]    DATETIME2(3)      NOT NULL,

        CONSTRAINT PK_EmployeeTrainingCode PRIMARY KEY CLUSTERED ([EmployeeId], [TrainingCode]),
        CONSTRAINT FK_EmployeeTrainingCode_Employee FOREIGN KEY ([EmployeeId])
            REFERENCES [EmployeeDomain].[Employee] ([EmployeeId])
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EmployeeCertification' AND schema_id = SCHEMA_ID('EmployeeDomain'))
BEGIN
    CREATE TABLE [EmployeeDomain].[EmployeeCertification]
    (
        [EmployeeCertificationId]  UNIQUEIDENTIFIER  NOT NULL CONSTRAINT DF_EmployeeCertification_Id DEFAULT NEWSEQUENTIALID(),
        [EmployeeId]                UNIQUEIDENTIFIER  NOT NULL,
        [CertificationCode]         NVARCHAR(50)      NOT NULL,
        [IssuingBody]               NVARCHAR(200)     NOT NULL,
        [IssuedAtUtc]               DATETIME2(3)      NOT NULL,
        [ExpiresAtUtc]              DATETIME2(3)      NULL,

        [CreatedUtc]                DATETIME2(3)      NOT NULL CONSTRAINT DF_EmployeeCertification_CreatedUtc DEFAULT SYSUTCDATETIME(),
        [CreatedBy]                 NVARCHAR(128)     NOT NULL CONSTRAINT DF_EmployeeCertification_CreatedBy DEFAULT SYSTEM_USER,
        [IsDeleted]                 BIT               NOT NULL CONSTRAINT DF_EmployeeCertification_IsDeleted DEFAULT (0),

        CONSTRAINT PK_EmployeeCertification PRIMARY KEY CLUSTERED ([EmployeeCertificationId]),
        CONSTRAINT FK_EmployeeCertification_Employee FOREIGN KEY ([EmployeeId])
            REFERENCES [EmployeeDomain].[Employee] ([EmployeeId])
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EmployeeExposureRecord' AND schema_id = SCHEMA_ID('EmployeeDomain'))
BEGIN
    CREATE TABLE [EmployeeDomain].[EmployeeExposureRecord]
    (
        [EmployeeExposureRecordId]  UNIQUEIDENTIFIER  NOT NULL CONSTRAINT DF_EmployeeExposureRecord_Id DEFAULT NEWSEQUENTIALID(),
        [EmployeeId]                 UNIQUEIDENTIFIER  NOT NULL,
        [HazardTypeCode]             NVARCHAR(30)      NOT NULL,
        [Description]                NVARCHAR(500)     NOT NULL,
        [ExposedAtUtc]               DATETIME2(3)      NOT NULL,

        [CreatedUtc]                 DATETIME2(3)      NOT NULL CONSTRAINT DF_EmployeeExposureRecord_CreatedUtc DEFAULT SYSUTCDATETIME(),
        [CreatedBy]                  NVARCHAR(128)     NOT NULL CONSTRAINT DF_EmployeeExposureRecord_CreatedBy DEFAULT SYSTEM_USER,
        [IsDeleted]                  BIT               NOT NULL CONSTRAINT DF_EmployeeExposureRecord_IsDeleted DEFAULT (0),

        CONSTRAINT PK_EmployeeExposureRecord PRIMARY KEY CLUSTERED ([EmployeeExposureRecordId]),
        CONSTRAINT FK_EmployeeExposureRecord_Employee FOREIGN KEY ([EmployeeId])
            REFERENCES [EmployeeDomain].[Employee] ([EmployeeId]),
        CONSTRAINT FK_EmployeeExposureRecord_HazardType FOREIGN KEY ([HazardTypeCode])
            REFERENCES [Lookup].[HazardType] ([Code])
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EmployeeInjuryEvent' AND schema_id = SCHEMA_ID('EmployeeDomain'))
BEGIN
    CREATE TABLE [EmployeeDomain].[EmployeeInjuryEvent]
    (
        [EmployeeInjuryEventId]  UNIQUEIDENTIFIER  NOT NULL CONSTRAINT DF_EmployeeInjuryEvent_Id DEFAULT NEWSEQUENTIALID(),
        [EmployeeId]              UNIQUEIDENTIFIER  NOT NULL,
        [Description]             NVARCHAR(500)     NOT NULL,
        [OccurredAtUtc]           DATETIME2(3)      NOT NULL,

        [CreatedUtc]              DATETIME2(3)      NOT NULL CONSTRAINT DF_EmployeeInjuryEvent_CreatedUtc DEFAULT SYSUTCDATETIME(),
        [CreatedBy]               NVARCHAR(128)     NOT NULL CONSTRAINT DF_EmployeeInjuryEvent_CreatedBy DEFAULT SYSTEM_USER,
        [IsDeleted]               BIT               NOT NULL CONSTRAINT DF_EmployeeInjuryEvent_IsDeleted DEFAULT (0),

        CONSTRAINT PK_EmployeeInjuryEvent PRIMARY KEY CLUSTERED ([EmployeeInjuryEventId]),
        CONSTRAINT FK_EmployeeInjuryEvent_Employee FOREIGN KEY ([EmployeeId])
            REFERENCES [EmployeeDomain].[Employee] ([EmployeeId])
    );
END
GO

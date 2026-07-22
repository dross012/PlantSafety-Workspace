-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         EmployeeSafety.HazardCommunicationTraining
--  Layer:          Infrastructure - SQL Scripts
--  File:           HazardCommunicationTraining.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the HazardCommunicationTraining aggregate root backing IHazardCommunicationTrainingRepository.
--                  TenantId is NOT NULL so this table is automatically picked up by the
--                  live-discovery Row-Level-Security policy in 90-row-level-security.sql
--                  (Security.fn_TenantAccessPredicate).
--                  Additive-only evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'EmployeeSafety')
BEGIN
    EXEC('CREATE SCHEMA [EmployeeSafety]');
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'HazardCommunicationTraining' AND schema_id = SCHEMA_ID('EmployeeSafety'))
BEGIN
    CREATE TABLE [EmployeeSafety].[HazardCommunicationTraining] (
        [HazardCommunicationTrainingId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SiteId] UNIQUEIDENTIFIER NULL,
        [Status] NVARCHAR(100) NOT NULL,
        [ProgramTitle] NVARCHAR(400) NOT NULL,
        [HazardClass] NVARCHAR(100) NOT NULL,
        [SdsReference] NVARCHAR(400) NOT NULL,
        [TrainedByUserId] UNIQUEIDENTIFIER NOT NULL,
        [TrainingDeliveredAtUtc] DATETIME2(3) NULL,
        [EmployeesTrainedCount] INT NOT NULL CONSTRAINT DF_HazardCommunicationTraining_EmployeesTrainedCount DEFAULT (0),
        [ComprehensionScore] DECIMAL(9,2) NULL,
        [PassingScoreThreshold] DECIMAL(9,2) NOT NULL,
        [NextTrainingDueAtUtc] DATETIME2(3) NULL,
        [CreatedAtUtc] DATETIME2(3) NOT NULL,
        [CreatedBy] UNIQUEIDENTIFIER NOT NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_HazardCommunicationTraining_IsDeleted DEFAULT (0),
        [ModifiedAtUtc] DATETIME2(3) NULL,
        [ModifiedBy] UNIQUEIDENTIFIER NULL,
        [CorrelationId] UNIQUEIDENTIFIER NULL,
        [ActorId] UNIQUEIDENTIFIER NULL
    );

    CREATE NONCLUSTERED INDEX [IX_HazardCommunicationTraining_TenantId_SiteId]
        ON [EmployeeSafety].[HazardCommunicationTraining] ([TenantId], [SiteId])
        INCLUDE ([Status], [ProgramTitle]);
END
GO

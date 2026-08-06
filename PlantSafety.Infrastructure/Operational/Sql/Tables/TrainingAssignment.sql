-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Features.Operational.Training
--  Layer:          Infrastructure – SQL Scripts
--  File:           TrainingAssignment.sql
--  Created:        2026-08-06
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Table DDL for the WorkerTrainingRecord aggregate, backing ITrainingRepository /
--                  TrainingDapperRepository (PlantSafety.Infrastructure.Features.Operational.
--                  Training.Repositories). DISTINCT from [Operational].[Training] (owned by a
--                  DIFFERENT ITrainingRepository -- PlantSafety.Application.Operational.Training.
--                  Repositories -- via usp_Training_List/sp_Training_*), and from three more
--                  unrelated ITrainingRepository interfaces found under Analytics.ScoreCards,
--                  Compliance.Certification, and Compliance.RegulatoryCalendar.Renewals (five
--                  total in this codebase). Named TrainingAssignment rather than reusing Training
--                  to stay unambiguous. Column list mirrors TrainingReadModel and the
--                  StoredProcedureBuilder Add* calls in TrainingDapperRepository.AssignAsync
--                  exactly. Status is documented as Assigned/InProgress/Completed/Failed/Expired;
--                  AssignAsync never sets it, so new rows start 'Assigned' (no lifecycle-transition
--                  surface exists on this feature beyond assignment + read). Additive-only
--                  evolution.
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TrainingAssignment' AND schema_id = SCHEMA_ID('Operational'))
BEGIN
    CREATE TABLE [Operational].[TrainingAssignment] (
        RecordId               UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_TrainingAssignment PRIMARY KEY,
        TenantId                UNIQUEIDENTIFIER NOT NULL,
        WorkerId                UNIQUEIDENTIFIER NOT NULL,
        TrainingCourseCode      NVARCHAR(50)     NOT NULL,
        TrainingCourseName      NVARCHAR(200)    NOT NULL,
        DeliveryMethod          NVARCHAR(50)     NOT NULL,
        Status                  NVARCHAR(30)     NOT NULL CONSTRAINT DF_TrainingAssignment_Status DEFAULT (N'Assigned'),
        DueDateUtc              DATETIME2(3)     NOT NULL,
        CompletedAtUtc          DATETIME2(3)     NULL,
        ExpiresAtUtc            DATETIME2(3)     NULL,
        Score                   INT              NULL,
        CreatedAtUtc            DATETIME2(3)     NOT NULL,
        CreatedBy               UNIQUEIDENTIFIER NOT NULL,
        LastModifiedByActorId   UNIQUEIDENTIFIER NULL,
        LastModifiedAtUtc       DATETIME2(3)     NOT NULL,
        RowVersion              ROWVERSION       NOT NULL
    );

    CREATE NONCLUSTERED INDEX IX_TrainingAssignment_TenantId_WorkerId ON [Operational].[TrainingAssignment] (TenantId, WorkerId);
    CREATE NONCLUSTERED INDEX IX_TrainingAssignment_TenantId_DueDateUtc ON [Operational].[TrainingAssignment] (TenantId, DueDateUtc);
END
GO

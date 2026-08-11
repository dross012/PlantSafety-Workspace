-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Checklists.ChecklistTemplate
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_ChecklistTemplate_Update.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Full-row update procedure for [Checklists].[ChecklistTemplate], backing
--                  IChecklistTemplateRepository.UpdateAsync (AddItem/Publish/Retire
--                  lifecycle transitions). Standard error envelope: TRY/CATCH with
--                  transaction rollback and THROW.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Checklists].[usp_ChecklistTemplate_Update]
    @TenantId             UNIQUEIDENTIFIER,
    @CorrelationId        UNIQUEIDENTIFIER,
    @ActorId              UNIQUEIDENTIFIER,
    @NowUtc               DATETIME2(3),
    @ChecklistTemplateId  UNIQUEIDENTIFIER,
    @Status               NVARCHAR(20),
    @TemplateTitle        NVARCHAR(200),
    @Description          NVARCHAR(2000),
    @ItemCount            INT,
    @IsMandatory          BIT,
    @RegulatoryReference  NVARCHAR(200) = NULL,
    @PublishedAtUtc       DATETIME2(3) = NULL,
    @RetiredAtUtc         DATETIME2(3) = NULL,
    @RetirementReason     NVARCHAR(500) = NULL
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [Checklists].[ChecklistTemplate]
        SET [Status]              = @Status,
            [TemplateTitle]       = @TemplateTitle,
            [Description]         = @Description,
            [ItemCount]           = @ItemCount,
            [IsMandatory]         = @IsMandatory,
            [RegulatoryReference] = @RegulatoryReference,
            [PublishedAtUtc]      = @PublishedAtUtc,
            [RetiredAtUtc]        = @RetiredAtUtc,
            [RetirementReason]    = @RetirementReason,
            [LastModifiedAtUtc]   = @NowUtc
        WHERE [ChecklistTemplateId] = @ChecklistTemplateId
          AND [TenantId]            = @TenantId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
        -- Canonical audit trail (Handbook §2.11): the write is recorded after it commits, in its
        -- own TRY/CATCH so an audit failure can never fail the operation it is describing.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'ChecklistTemplate', DB_NAME(), N'UPDATE', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'ChecklistTemplateId=', @ChecklistTemplateId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ChecklistTemplateId=', @ChecklistTemplateId, N'; User=', SYSTEM_USER);
    DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

    INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
    VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));

    THROW;
END CATCH;
GO

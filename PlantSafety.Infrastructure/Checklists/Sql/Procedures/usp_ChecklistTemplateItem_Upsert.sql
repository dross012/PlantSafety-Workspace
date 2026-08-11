-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Checklists.ChecklistTemplate
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_ChecklistTemplateItem_Upsert.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Idempotent insert-if-not-exists for [Checklists].[ChecklistTemplateItem].
--                  Called once per in-memory item for every AddAsync/UpdateAsync so that
--                  newly-added ChecklistTemplateItem child entities (added via
--                  ChecklistTemplate.AddItem()) are persisted without a separate
--                  Application-layer "AddItem" repository call. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Checklists].[usp_ChecklistTemplateItem_Upsert]
    @TenantId             UNIQUEIDENTIFIER,
    @CorrelationId        UNIQUEIDENTIFIER,
    @ActorId              UNIQUEIDENTIFIER,
    @NowUtc               DATETIME2(3),
    @ItemId               UNIQUEIDENTIFIER,
    @ChecklistTemplateId  UNIQUEIDENTIFIER,
    @Sequence             INT,
    @Description          NVARCHAR(1000),
    @IsCritical           BIT,
    @RegulatoryReference  NVARCHAR(200) = NULL,
    @AddedAtUtc           DATETIME2(3)
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        IF NOT EXISTS (SELECT 1 FROM [Checklists].[ChecklistTemplateItem] WHERE [ItemId] = @ItemId)
        BEGIN
            INSERT INTO [Checklists].[ChecklistTemplateItem]
                ([ItemId], [ChecklistTemplateId], [Sequence], [Description], [IsCritical],
                 [RegulatoryReference], [AddedAtUtc])
            VALUES
                (@ItemId, @ChecklistTemplateId, @Sequence, @Description, @IsCritical,
                 @RegulatoryReference, @AddedAtUtc);
        END
    COMMIT TRANSACTION;
        -- Canonical audit trail (Handbook section 2.11): recorded after the write, in its own
        -- TRY/CATCH so an audit failure can never fail the operation it describes.
        BEGIN TRY
            INSERT INTO [PlantSafety.AuditDB].dbo.AuditLog (TableName, SourceDatabase, Operation, ChangedBy, ChangeTime, ViewerAccessed, ErrorContext, Severity, Module)
            VALUES (N'ChecklistTemplateItem', DB_NAME(), N'INSERT', SYSTEM_USER, SYSUTCDATETIME(), 0, CONCAT(N'ItemId=', @ItemId), 0, OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
        END CATCH;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        DECLARE @__ErrorContext nvarchar(400) = CONCAT(N'ItemId=', @ItemId, N'; User=', SYSTEM_USER);
        DECLARE @__ErrorDetails nvarchar(max) = CONCAT(N'ErrorNumber=', ERROR_NUMBER(), N'; ErrorMessage=', ERROR_MESSAGE());

        BEGIN TRY
            INSERT INTO [PlantSafety.ErrorDB].dbo.ErrorLog (ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, ErrorDate, ErrorContext, ErrorParameters, SourceDatabase, Module)
            VALUES (ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), SYSUTCDATETIME(), @__ErrorContext, @__ErrorDetails, DB_NAME(), OBJECT_NAME(@@PROCID));
        END TRY
        BEGIN CATCH
            -- Error logging must never mask the original error
        END CATCH;

    THROW;
END CATCH;
GO

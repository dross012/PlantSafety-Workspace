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
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

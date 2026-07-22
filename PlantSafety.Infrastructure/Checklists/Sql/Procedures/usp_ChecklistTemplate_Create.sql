-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Checklists.ChecklistTemplate
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_ChecklistTemplate_Create.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Insert procedure for [Checklists].[ChecklistTemplate], backing
--                  IChecklistTemplateRepository.AddAsync. Standard error envelope:
--                  TRY/CATCH with transaction rollback and THROW.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Checklists].[usp_ChecklistTemplate_Create]
    @TenantId            UNIQUEIDENTIFIER,
    @CorrelationId       UNIQUEIDENTIFIER,
    @ActorId             UNIQUEIDENTIFIER,
    @NowUtc               DATETIME2(3),
    @ChecklistTemplateId  UNIQUEIDENTIFIER,
    @SiteId               UNIQUEIDENTIFIER = NULL,
    @Status               NVARCHAR(20),
    @TemplateTitle        NVARCHAR(200),
    @Category             NVARCHAR(100),
    @Description          NVARCHAR(2000),
    @ItemCount            INT,
    @IsMandatory          BIT,
    @RegulatoryReference  NVARCHAR(200) = NULL,
    @CreatedAtUtc         DATETIME2(3),
    @CreatedBy            UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [Checklists].[ChecklistTemplate]
            ([ChecklistTemplateId], [TenantId], [SiteId], [Status], [TemplateTitle], [Category],
             [Description], [ItemCount], [IsMandatory], [RegulatoryReference],
             [CreatedAtUtc], [CreatedBy], [LastModifiedAtUtc])
        VALUES
            (@ChecklistTemplateId, @TenantId, @SiteId, @Status, @TemplateTitle, @Category,
             @Description, @ItemCount, @IsMandatory, @RegulatoryReference,
             @CreatedAtUtc, @CreatedBy, @NowUtc);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

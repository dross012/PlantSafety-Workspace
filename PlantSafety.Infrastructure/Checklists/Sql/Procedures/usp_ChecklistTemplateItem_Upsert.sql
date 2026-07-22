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
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

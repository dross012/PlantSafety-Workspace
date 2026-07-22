-- ============================================================================
--  Project:        PlantSafety.Platform
--  Module:         Checklists.ChecklistCompletion
--  Layer:          Infrastructure - SQL Scripts
--  File:           usp_ChecklistCompletion_Create.sql
--  Created:        2026-07-21
--  Author:         PlantSafety Platform Team
--  SchemaVersion:  2026.1
--  Description:    Insert procedure for [Checklists].[ChecklistCompletion], backing
--                  IChecklistCompletionRepository.AddAsync. Standard error envelope:
--                  TRY/CATCH with transaction rollback and THROW.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [Checklists].[usp_ChecklistCompletion_Create]
    @TenantId               UNIQUEIDENTIFIER,
    @CorrelationId          UNIQUEIDENTIFIER,
    @ActorId                UNIQUEIDENTIFIER,
    @NowUtc                 DATETIME2(3),
    @ChecklistCompletionId  UNIQUEIDENTIFIER,
    @SiteId                 UNIQUEIDENTIFIER = NULL,
    @Status                 NVARCHAR(20),
    @ChecklistTemplateId    UNIQUEIDENTIFIER,
    @ItemsTotal             INT,
    @ItemsPassed            INT,
    @ItemsFailed            INT,
    @Outcome                NVARCHAR(30),
    @CreatedAtUtc           DATETIME2(3),
    @CreatedBy              UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [Checklists].[ChecklistCompletion]
            ([ChecklistCompletionId], [TenantId], [SiteId], [Status], [ChecklistTemplateId],
             [ItemsTotal], [ItemsPassed], [ItemsFailed], [Outcome],
             [CreatedAtUtc], [CreatedBy], [LastModifiedAtUtc])
        VALUES
            (@ChecklistCompletionId, @TenantId, @SiteId, @Status, @ChecklistTemplateId,
             @ItemsTotal, @ItemsPassed, @ItemsFailed, @Outcome,
             @CreatedAtUtc, @CreatedBy, @NowUtc);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

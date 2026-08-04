-- ============================================================================
--  Module: HazardousWasteManagement.AccumulationAreas | Layer: Infrastructure - SQL Scripts
--  File: usp_AccumulationAreas_Update.sql | Created: 2026-08-04
--  Description: Full-row update procedure for [HazardousWasteManagement].[AccumulationAreas]. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_AccumulationAreas_Update]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @NowUtc        DATETIME2(3),
    @AccumulationAreaId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @Status NVARCHAR(100)
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [HazardousWasteManagement].[AccumulationAreas]
        SET
            [Status] = @Status,
            [ModifiedAtUtc] = @NowUtc,
            [ModifiedBy] = @ActorId,
            [CorrelationId] = @CorrelationId
        WHERE [AccumulationAreaId] = @AccumulationAreaId
          AND [TenantId] = @TenantId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

-- ============================================================================
--  Module:         CommunitySafety.CommunityImpactAssessment
--  File:           usp_CommunityImpactAssessment_Update.sql
--  Created:        2026-08-04
--  Description:    Update procedure for [CommunitySafety].[CommunityImpactAssessment] lifecycle
--                  fields, backing ICommunityImpactAssessmentRepository.UpdateAsync.
--                  Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [CommunitySafety].[usp_CommunityImpactAssessment_Update]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @NowUtc        DATETIME2(3),
    @CommunityImpactAssessmentId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @FinalizedAtUtc DATETIME2(3) = NULL
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [CommunitySafety].[CommunityImpactAssessment]
        SET
            [Status] = @Status,
            [FinalizedAtUtc] = @FinalizedAtUtc,
            [ModifiedAtUtc] = @NowUtc,
            [ModifiedBy] = @ActorId,
            [CorrelationId] = @CorrelationId
        WHERE [CommunityImpactAssessmentId] = @CommunityImpactAssessmentId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

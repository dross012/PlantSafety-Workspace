-- ============================================================================
--  Module:         CommunitySafety.CommunityNotification
--  File:           usp_CommunityNotification_Update.sql
--  Created:        2026-08-04
--  Description:    Update procedure for [CommunitySafety].[CommunityNotification] lifecycle
--                  fields, backing ICommunityNotificationRepository.UpdateAsync. Additive-only
--                  evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [CommunitySafety].[usp_CommunityNotification_Update]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @NowUtc        DATETIME2(3),
    @CommunityNotificationId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @AcknowledgedAtUtc DATETIME2(3) = NULL,
    @FailureReason NVARCHAR(1000) = NULL
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [CommunitySafety].[CommunityNotification]
        SET
            [Status] = @Status,
            [AcknowledgedAtUtc] = @AcknowledgedAtUtc,
            [FailureReason] = @FailureReason,
            [ModifiedAtUtc] = @NowUtc,
            [ModifiedBy] = @ActorId,
            [CorrelationId] = @CorrelationId
        WHERE [CommunityNotificationId] = @CommunityNotificationId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

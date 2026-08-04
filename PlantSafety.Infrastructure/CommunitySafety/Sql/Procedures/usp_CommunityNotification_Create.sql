-- ============================================================================
--  Module:         CommunitySafety.CommunityNotification
--  File:           usp_CommunityNotification_Create.sql
--  Created:        2026-08-04
--  Description:    Insert procedure for [CommunitySafety].[CommunityNotification], backing
--                  ICommunityNotificationRepository.AddAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [CommunitySafety].[usp_CommunityNotification_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @CommunityNotificationId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50),
    @Channel NVARCHAR(50),
    @RecipientContactId UNIQUEIDENTIFIER,
    @Subject NVARCHAR(300),
    @MessageBody NVARCHAR(MAX),
    @RelatedConcernId UNIQUEIDENTIFIER = NULL,
    @RelatedIncidentId UNIQUEIDENTIFIER = NULL,
    @SentAtUtc DATETIME2(3),
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [CommunitySafety].[CommunityNotification]
            ([CommunityNotificationId], [TenantId], [SiteId], [Status], [Channel], [RecipientContactId], [Subject], [MessageBody], [RelatedConcernId], [RelatedIncidentId], [SentAtUtc], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@CommunityNotificationId, @TenantId, @SiteId, @Status, @Channel, @RecipientContactId, @Subject, @MessageBody, @RelatedConcernId, @RelatedIncidentId, @SentAtUtc, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

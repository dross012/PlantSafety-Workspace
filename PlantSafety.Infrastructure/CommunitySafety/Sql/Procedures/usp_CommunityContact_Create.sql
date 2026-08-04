-- ============================================================================
--  Module:         CommunitySafety.CommunityContact
--  File:           usp_CommunityContact_Create.sql
--  Created:        2026-08-04
--  Description:    Insert procedure for [CommunitySafety].[CommunityContact], backing
--                  ICommunityContactRepository.AddAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [CommunitySafety].[usp_CommunityContact_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @CommunityContactId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50),
    @DisplayName NVARCHAR(200),
    @ContactType NVARCHAR(50),
    @Organization NVARCHAR(200) = NULL,
    @CommunityArea NVARCHAR(200),
    @Email NVARCHAR(320),
    @Phone NVARCHAR(32) = NULL,
    @OptedInForNotifications BIT,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [CommunitySafety].[CommunityContact]
            ([CommunityContactId], [TenantId], [SiteId], [Status], [DisplayName], [ContactType], [Organization], [CommunityArea], [Email], [Phone], [OptedInForNotifications], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@CommunityContactId, @TenantId, @SiteId, @Status, @DisplayName, @ContactType, @Organization, @CommunityArea, @Email, @Phone, @OptedInForNotifications, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

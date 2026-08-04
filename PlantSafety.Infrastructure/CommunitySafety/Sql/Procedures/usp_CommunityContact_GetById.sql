-- ============================================================================
--  Module:         CommunitySafety.CommunityContact
--  File:           usp_CommunityContact_GetById.sql
--  Created:        2026-08-04
--  Description:    Returns a single CommunityContact row by identifier, backing
--                  ICommunityContactRepository.GetByIdAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [CommunitySafety].[usp_CommunityContact_GetById]
(
    @CommunityContactId UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT [CommunityContactId], [TenantId], [SiteId], [Status], [DisplayName], [ContactType], [Organization], [CommunityArea], [Email], [Phone], [OptedInForNotifications], [ArchivedAtUtc], [ArchivalReason], [CreatedAtUtc], [CreatedBy]
    FROM [CommunitySafety].[CommunityContact]
    WHERE [CommunityContactId] = @CommunityContactId
      AND ISNULL([IsDeleted], 0) = 0;
END;
GO

-- ============================================================================
--  Module:         CommunitySafety.CommunityConcern
--  File:           usp_CommunityConcern_Create.sql
--  Created:        2026-08-04
--  Description:    Insert procedure for [CommunitySafety].[CommunityConcern], backing
--                  ICommunityConcernRepository.AddAsync. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [CommunitySafety].[usp_CommunityConcern_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @CommunityConcernId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER,
    @Status NVARCHAR(50),
    @Category NVARCHAR(50),
    @Severity NVARCHAR(30),
    @FacilityArea NVARCHAR(200),
    @Description NVARCHAR(4000),
    @ReportedByContactId UNIQUEIDENTIFIER = NULL,
    @RelatedIncidentId UNIQUEIDENTIFIER = NULL,
    @LoggedAtUtc DATETIME2(3),
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [CommunitySafety].[CommunityConcern]
            ([CommunityConcernId], [TenantId], [SiteId], [Status], [Category], [Severity], [FacilityArea], [Description], [ReportedByContactId], [RelatedIncidentId], [LoggedAtUtc], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@CommunityConcernId, @TenantId, @SiteId, @Status, @Category, @Severity, @FacilityArea, @Description, @ReportedByContactId, @RelatedIncidentId, @LoggedAtUtc, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

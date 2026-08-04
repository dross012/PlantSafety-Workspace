-- ============================================================================
--  Module: HazardousWasteManagement.Transporters | Layer: Infrastructure - SQL Scripts
--  File: usp_Transporters_Create.sql | Created: 2026-08-04
--  Description: Insert procedure for [HazardousWasteManagement].[Transporters]. Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [HazardousWasteManagement].[usp_Transporters_Create]
    @CorrelationId UNIQUEIDENTIFIER = NULL,
    @ActorId       UNIQUEIDENTIFIER = NULL,
    @TransporterId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(100),
    @CompanyName NVARCHAR(400),
    @EpaTransporterId NVARCHAR(50),
    @LicenseExpiresAtUtc DATETIME2(3) = NULL,
    @CreatedAtUtc DATETIME2(3),
    @CreatedBy UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        INSERT INTO [HazardousWasteManagement].[Transporters]
            ([TransporterId], [TenantId], [SiteId], [Status], [CompanyName], [EpaTransporterId], [LicenseExpiresAtUtc], [CreatedAtUtc], [CreatedBy], [CorrelationId], [ActorId])
        VALUES
            (@TransporterId, @TenantId, @SiteId, @Status, @CompanyName, @EpaTransporterId, @LicenseExpiresAtUtc, @CreatedAtUtc, @CreatedBy, @CorrelationId, @ActorId);
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

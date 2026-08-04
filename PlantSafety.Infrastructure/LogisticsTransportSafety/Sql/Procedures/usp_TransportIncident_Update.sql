-- ============================================================================
--  Module:         LogisticsTransportSafety.TransportIncidents
--  File:           usp_TransportIncident_Update.sql
--  Created:        2026-08-04
--  Description:    Update procedure for [LogisticsTransportSafety].[TransportIncidents], backing
--                  ITransportIncidentRepository.UpdateAsync. Covers BeginInvestigation and Close
--                  -- matches the exact parameter set sent by
--                  TransportIncidentDapperRepository.UpdateAsync (Status only; incident-report
--                  facts are immutable post-creation). Additive-only evolution.
-- ============================================================================

CREATE OR ALTER PROCEDURE [LogisticsTransportSafety].[usp_TransportIncident_Update]
    @NowUtc DATETIME2(3),
    @TransportIncidentId UNIQUEIDENTIFIER,
    @TenantId UNIQUEIDENTIFIER,
    @SiteId UNIQUEIDENTIFIER = NULL,
    @Status NVARCHAR(50)
AS
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRANSACTION;
        UPDATE [LogisticsTransportSafety].[TransportIncidents]
        SET
            [SiteId] = @SiteId,
            [Status] = @Status,
            [ModifiedAtUtc] = @NowUtc
        WHERE [TransportIncidentId] = @TransportIncidentId
          AND [TenantId] = @TenantId
          AND ISNULL([IsDeleted], 0) = 0;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO

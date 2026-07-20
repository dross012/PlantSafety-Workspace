-- =============================================
-- Migration:     92-always-encrypted-prep.sql
-- Author:        Governance Enforcer Agent
-- Create date:   2026-07-19
-- Description:   Always Encrypted infrastructure PREP ONLY (Column Master
--                Key + Column Encryption Key). Governance remediation item
--                #3 (security hardening beyond RBAC).
--
-- *** No genuine Always Encrypted column candidate exists today ***
-- A full scan of all 10 in-scope databases for column names matching
-- SSN/Password/CreditCard/Payment/CVV/BankAccount/Secret/TaxId/
-- NationalId/DateOfBirth/Salary/SocialSecurity returned zero genuine
-- matches (one incidental substring hit on
-- ProcessManagement.ProcessDefinition.ProcessName, which is a business-
-- process name, not personal/financial/credential data -- not a
-- candidate). The nine columns classified in Phase 7
-- (72-data-classification.sql) and masked in 91-dynamic-data-masking.sql
-- are free-text incident/injury narrative and a risk-level enum -- exactly
-- the category Dynamic Data Masking is designed for (SP-layer/app still
-- needs to read and reason over the real text; DDM is the correct control
-- there, not Always Encrypted, which would make the data opaque to the
-- database engine entirely, including to the SPs that currently filter,
-- join, and search on the DB side). Per the task's explicit instruction,
-- Always Encrypted is NOT applied to any column here -- this script builds
-- the CMK/CEK infrastructure available-but-unused, exactly analogous to
-- the Phase 7 TDE prep pattern (71-tde-encryption.sql) for a feature this
-- platform doesn't need yet but may need later.
--
-- Future-candidate columns (if this platform ever stores these -- NOT
-- built here, flagged for a future, human-directed pass):
--   * A genuine worker SSN / national ID field, if ever added.
--   * Payment/bank-account details, if ever added (e.g. contractor payment
--     processing).
--   * Any stored password/credential/API-secret column (note: this
--     platform correctly has none today -- CloudIdentityDB models Azure/ARM
--     resource identity, not password storage).
--   * Date of birth, if ever added to a worker profile (deterministic
--     Always Encrypted would be wrong for DOB given re-identification risk
--     from a small value space -- randomized AE would be the correct
--     choice if this is ever added).
--
-- *** How this CMK/CEK were provisioned ***
-- Always Encrypted's Column Master Key is a CLIENT-SIDE secret -- SQL
-- Server never sees the CMK private key, only key metadata (key store
-- provider name + key path). This means, unlike a plain SQL Server
-- certificate (used for TDE), the CMK here is a Windows certificate-store
-- certificate (self-signed, 2048-bit RSA, CurrentUser\My store on this dev
-- box, 10-year validity), and the CEK's ENCRYPTED_VALUE below was computed
-- OUTSIDE SQL Server using Microsoft.Data.SqlClient's
-- SqlColumnEncryptionCertificateStoreProvider (the same provider class the
-- .NET driver uses at runtime), via a small throwaway console program:
--   1. New-SelfSignedCertificate (PowerShell, built-in PKI module) created
--      the CMK certificate in Cert:\CurrentUser\My.
--      Thumbprint: CCE6FD113DFBE14C0B3119034AD13B21A562B363
--   2. Exported for backup/DR to (dev-only path, same production warning
--      as the Phase 7 TDE cert backup applies -- a real deployment must
--      use Key Vault/HSM, never a repo-adjacent folder):
--        PlantSafety.Infrastructure\Persistence\Migrations\Phase9-Security\
--        always-encrypted-cmk-backup\PlantSafety_AlwaysEncrypted_CMK.cer  (public)
--        always-encrypted-cmk-backup\PlantSafety_AlwaysEncrypted_CMK.pfx  (private, PFX
--          password: PvkBackup_DevOnly_2026!ChangeMe -- dev-only literal per the same
--          convention already used and flagged in 71-tde-encryption.sql)
--   3. A random 32-byte plaintext CEK was generated and encrypted with
--      SqlColumnEncryptionCertificateStoreProvider.EncryptColumnEncryptionKey
--      against that certificate (RSA_OAEP). Round-trip decrypt was verified
--      programmatically to match the original plaintext before use below --
--      this ENCRYPTED_VALUE is a genuinely valid, working Always Encrypted
--      CEK for this CMK, not a placeholder.
--   4. Since no column uses this CEK, the plaintext CEK bytes were not
--      persisted anywhere (dev console output only, not written to disk or
--      committed) -- there is nothing to protect yet; regenerate CEK/CMK
--      when a real candidate column is identified rather than reusing this
--      placeholder pair for production data.
--
-- This CMK/CEK pair is intentionally UNUSED (no column references it via
-- ENCRYPTED WITH). It exists purely so a future genuine candidate column
-- can be enabled quickly (ALTER TABLE ... ALTER COLUMN ... ENCRYPTED WITH
-- (COLUMN_ENCRYPTION_KEY = CEK_AlwaysEncrypted_Placeholder, ENCRYPTION_TYPE
-- = ..., ALGORITHM = 'AEAD_AES_256_CBC_HMAC_SHA_256') once a real
-- candidate exists and a human has confirmed the deterministic-vs-
-- randomized choice for that column.
--
-- Idempotency: CMK/CEK creation is existence-checked; safe to re-run.
-- Applied to PlantSafety.CoreDB only (where the classified/masked columns
-- live and where any future worker-PII candidate would most likely land).
-- =============================================
SET NOCOUNT ON;
GO
USE [PlantSafety.CoreDB];
GO

------------------------------------------------------------
-- 1. Column Master Key definition (metadata only -- references the
--    Windows certificate store; SQL Server never holds the private key).
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.column_master_keys WHERE name = 'CMK_AlwaysEncrypted_Placeholder')
BEGIN
    CREATE COLUMN MASTER KEY CMK_AlwaysEncrypted_Placeholder
    WITH (
        KEY_STORE_PROVIDER_NAME = N'MSSQL_CERTIFICATE_STORE',
        KEY_PATH = N'CurrentUser/My/CCE6FD113DFBE14C0B3119034AD13B21A562B363'
    );
    PRINT 'CMK_AlwaysEncrypted_Placeholder created.';
END
ELSE
    PRINT 'CMK_AlwaysEncrypted_Placeholder already exists -- skipped.';
GO

------------------------------------------------------------
-- 2. Column Encryption Key, wrapped by the CMK above.
--    ENCRYPTED_VALUE below is a real, round-trip-verified RSA_OAEP
--    encryption of a random 32-byte key under the CMK certificate
--    (computed client-side, see header) -- not a placeholder string.
------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.column_encryption_keys WHERE name = 'CEK_AlwaysEncrypted_Placeholder')
BEGIN
    CREATE COLUMN ENCRYPTION KEY CEK_AlwaysEncrypted_Placeholder
    WITH VALUES (
        COLUMN_MASTER_KEY = CMK_AlwaysEncrypted_Placeholder,
        ALGORITHM = 'RSA_OAEP',
        ENCRYPTED_VALUE = 0x016E000001630075007200720065006E00740075007300650072002F006D0079002F006300630065003600660064003100310033006400660062006500310034006300300062003300310031003900300033003400610064003100330062003200310061003500360032006200330036003300315AB074E0D8830F29D47D19ED556812BEA86E757492832A9C038BA47BC62578C3E74EB742EC5D19B962C3A9576FF39B305BCA13517B54738509173F0391C53A4EEE4577CDA219AAD1647EF288F1038CC2D51CE73FB550E0C54B81CC0AA85A718C5C6F0C1BA126D9E31437BE03E3CD40D158BB44A0BCF618440EE4DA446549B779D9CA75509678D6B334317FA45EC07BD8C3D559380E5D87B5DEA9829D10E5638772D52F9A653E3940C8938638B8E93429D73F95BE0F895A039848B8188A47EBA41D8F01528B07E28151E9A36C46F1CC429BA73B030328F7972EF734C6F8279BEE316FEB13EF072BAF83093A7254054BD62BCD4277C5AE875A808373AB7FC07A601196BBC18479CDB0147A6ED77E22AB66552C8C3986AE0321F931F7DBEF18C0BD36331D171485E0F966F52E21A5F88E6C6A5ABC4127DCFB6DC19D9150F0C5BF5669B278EA6F7E3702FD729EDD42A74B26DCE9D8B278D2D8DB946521CFE190B3D0BF8D11DB133120824AECDCBDD0F8A2B69336BA01B3071D5A675239466C4F0DBCA3FF329958089144DA26262916D86B770DA31587D5EE7B8026DD56E25E80BF71B6CC53A03CB07A83035A67D48892218E024B5A9CFA248D7B7382BA6C72876E3BE23A3CE323FD94119FD211C48EF8CB0563D94E8443D3F4AD9E266DBF3130938C79376BAD890A6E961718E8890EAF08B232924A22FF83A91731C998D8999F71
    );
    PRINT 'CEK_AlwaysEncrypted_Placeholder created.';
END
ELSE
    PRINT 'CEK_AlwaysEncrypted_Placeholder already exists -- skipped.';
GO

------------------------------------------------------------
-- 3. Verification -- confirm the CMK/CEK exist and are NOT referenced by
--    any column (proving this is prep-only, nothing was force-encrypted).
------------------------------------------------------------
SELECT name, key_store_provider_name, key_path FROM sys.column_master_keys;
SELECT cek.name, cekv.encryption_algorithm_name, DATALENGTH(cekv.encrypted_value) AS EncryptedValueBytes
FROM sys.column_encryption_keys cek
JOIN sys.column_encryption_key_values cekv ON cek.column_encryption_key_id = cekv.column_encryption_key_id;
SELECT COUNT(*) AS ColumnsCurrentlyUsingAlwaysEncrypted
FROM sys.columns WHERE column_encryption_key_id IS NOT NULL;
GO

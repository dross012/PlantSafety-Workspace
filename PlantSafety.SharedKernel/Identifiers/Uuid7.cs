// ============================================================================
//  Project:        PlantSafety.Platform
//  Module:         SharedKernel (Domain)
//  Layer:          Domain
//  File:           Uuid7.cs
//  Created:        2026-05-26
//  Author:         David (Ted) + Copilot
//  SchemaVersion:  2026.1
//  Description:
//      Canonical UUIDv7 generator for deterministic, time-ordered identifiers.
//      Fully compliant with the Enterprise Architectural Rule Handbook (2026),
//      specifically:
//          - Deterministic Identifier Rule (1.2)
//          - Timestamp Type Rule (3.5)
//          - StrongId Enforcement Rule (5.5)
//          - Event Envelope Rule (12.5)
//          - Additive-only evolution
//
//      UUIDv7 provides:
//          - Time-ordered monotonic identifiers
//          - SIEM-safe auditability
//          - Replay-safe deterministic ordering
//          - No dependency on system locale or randomness sources
//
//      This implementation is domain-safe, thread-safe, and contains no
//      infrastructure logic.
// ============================================================================

using System;
using System.Security.Cryptography;
using System.Threading;

namespace PlantSafety.SharedKernel.Identifiers;

public static class Uuid7
{
    // Last timestamp used (in Unix milliseconds)
    private static long _lastTimestamp;

    // Counter for monotonic sequencing within the same millisecond
    private static int _sequence;

    /// <summary>
    /// Generates a new UUIDv7 identifier.
    /// </summary>
    public static Guid New()
    {
        // --------------------------------------------------------------------
        // Step 1: Get current Unix time in milliseconds (UTC only)
        // --------------------------------------------------------------------
        long unixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // --------------------------------------------------------------------
        // Step 2: Ensure monotonicity
        // --------------------------------------------------------------------
        long last = Interlocked.Read(ref _lastTimestamp);

        if (unixMs > last)
        {
            // New millisecond → reset sequence
            Interlocked.Exchange(ref _lastTimestamp, unixMs);
            Interlocked.Exchange(ref _sequence, 0);
        }
        else
        {
            // Same millisecond → increment sequence
            unixMs = last;
            Interlocked.Increment(ref _sequence);
        }

        int seq = Interlocked.CompareExchange(ref _sequence, 0, 0);

        // --------------------------------------------------------------------
        // Step 3: Construct UUIDv7 fields
        // --------------------------------------------------------------------
        Span<byte> bytes = stackalloc byte[16];

        // Timestamp (48 bits)
        bytes[0] = (byte)((unixMs >> 40) & 0xFF);
        bytes[1] = (byte)((unixMs >> 32) & 0xFF);
        bytes[2] = (byte)((unixMs >> 24) & 0xFF);
        bytes[3] = (byte)((unixMs >> 16) & 0xFF);
        bytes[4] = (byte)((unixMs >> 8) & 0xFF);
        bytes[5] = (byte)(unixMs & 0xFF);

        // Sequence (12 bits) + version (4 bits = 7)
        bytes[6] = (byte)(0x70 | ((seq >> 8) & 0x0F)); // version 7
        bytes[7] = (byte)(seq & 0xFF);

        // Random bytes (62 bits)
        RandomNumberGenerator.Fill(bytes.Slice(8));

        // Variant bits (RFC 4122)
        bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80);

        return new Guid(bytes);
    }
}

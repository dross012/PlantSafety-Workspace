// ============================================================================
//  Project:        Shared.AiInference
//  Module:         Abstractions.Envelope
//  File:           AiInferenceHasher.cs
//  Created:        2026-08-09
//  Description:    Deterministically hashes a serialized input/output
//                  payload for AiInferenceEnvelope. SHA-256 over the UTF8
//                  bytes of the caller-supplied JSON string -- the caller
//                  is responsible for deterministic serialization (stable
//                  property order, no timestamps embedded in the hashed
//                  payload), this type only hashes what it's given.
// ============================================================================
using System.Security.Cryptography;
using System.Text;

namespace Shared.AiInference.Abstractions.Envelope;

/// <summary>Deterministically hashes a serialized payload for the AI inference envelope.</summary>
public static class AiInferenceHasher
{
    /// <summary>Returns the lowercase hex SHA-256 hash of the given UTF8 string.</summary>
    public static string Hash(string payload)
    {
        ArgumentNullException.ThrowIfNull(payload);
        byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}

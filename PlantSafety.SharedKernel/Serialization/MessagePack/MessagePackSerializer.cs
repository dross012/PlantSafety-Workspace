using PlantSafety.SharedKernel.Serialization.Abstractions;
using global::MessagePack; // Add this using directive for MessagePack

namespace PlantSafety.SharedKernel.Serialization.MessagePack;

public sealed class MessagePackSerializer : ISerializer
{
    private static readonly MessagePackSerializer _instance = new MessagePackSerializer();

    public byte[] Serialize<T>(T value)
        => _instance.Serialize(value);

    public T? Deserialize<T>(byte[] data)
        => _instance.Deserialize<T>(data);

    public object? Deserialize(byte[] data, Type type)
        => MessagePackSerializer.NonGeneric.Deserialize(type, data);

    // Add static Serialize/Deserialize methods if needed, or clarify usage.
}

using PlantSafety.SharedKernel.Serialization.Abstractions;

namespace PlantSafety.SharedKernel.Serialization.MessagePack;

public sealed class MessagePackSerializer : ISerializer
{
    public byte[] Serialize<T>(T value)
        => global::MessagePack.MessagePackSerializer.Serialize(value);

    public T? Deserialize<T>(byte[] data)
        => global::MessagePack.MessagePackSerializer.Deserialize<T>(data);

    public object? Deserialize(byte[] data, Type type)
        => global::MessagePack.MessagePackSerializer.Deserialize(type, data);
}

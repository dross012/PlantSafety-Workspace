using MongoDB.Bson.IO;
using PlantSafety.SharedKernel.Serialization.Abstractions;

namespace PlantSafety.SharedKernel.Serialization.Bson;

public sealed class BsonSerializer : ISerializer
{
    public byte[] Serialize<T>(T value)
    {
        using var ms = new MemoryStream();
        using var writer = new BsonBinaryWriter(ms);
        MongoDB.Bson.Serialization.BsonSerializer.Serialize(writer, value!.GetType(), value);
        writer.Flush();
        return ms.ToArray();
    }

    public T? Deserialize<T>(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var reader = new BsonBinaryReader(ms);
        return (T?)MongoDB.Bson.Serialization.BsonSerializer.Deserialize(reader, typeof(T));
    }

    public object? Deserialize(byte[] data, Type type)
    {
        using var ms = new MemoryStream(data);
        using var reader = new BsonBinaryReader(ms);
        return MongoDB.Bson.Serialization.BsonSerializer.Deserialize(reader, type);
    }
}

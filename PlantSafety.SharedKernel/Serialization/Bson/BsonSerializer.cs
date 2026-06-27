using System.Formats.Bson;
using PlantSafety.SharedKernel.Serialization.Abstractions;

namespace PlantSafety.SharedKernel.Serialization.Bson;

public sealed class BsonSerializer : ISerializer
{
    public byte[] Serialize<T>(T value)
    {
        using var ms = new MemoryStream();
        using var writer = new BsonWriter(ms);
        System.Text.Json.JsonSerializer.Serialize(writer, value);
        return ms.ToArray();
    }

    public T? Deserialize<T>(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var reader = new BsonReader(ms);
        return System.Text.Json.JsonSerializer.Deserialize<T>(reader);
    }

    public object? Deserialize(byte[] data, Type type)
    {
        using var ms = new MemoryStream(data);
        using var reader = new BsonReader(ms);
        return System.Text.Json.JsonSerializer.Deserialize(reader, type);
    }
}

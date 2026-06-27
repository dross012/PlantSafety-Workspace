using System.Text;
using System.Xml.Serialization;
using PlantSafety.SharedKernel.Serialization.Abstractions;

namespace PlantSafety.SharedKernel.Serialization.Xml;

public sealed class XmlSerializer : ISerializer
{
    public byte[] Serialize<T>(T value)
    {
        var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
        using var ms = new MemoryStream();
        serializer.Serialize(ms, value);
        return ms.ToArray();
    }

    public T? Deserialize<T>(byte[] data)
    {
        var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
        using var ms = new MemoryStream(data);
        return (T?)serializer.Deserialize(ms);
    }

    public object? Deserialize(byte[] data, Type type)
    {
        var serializer = new System.Xml.Serialization.XmlSerializer(type);
        using var ms = new MemoryStream(data);
        return serializer.Deserialize(ms);
    }
}

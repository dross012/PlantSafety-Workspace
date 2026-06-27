namespace PlantSafety.SharedKernel.Serialization.Abstractions;

public interface ISerializer
{
    byte[] Serialize<T>(T value);
    T? Deserialize<T>(byte[] data);
    object? Deserialize(byte[] data, Type type);
}

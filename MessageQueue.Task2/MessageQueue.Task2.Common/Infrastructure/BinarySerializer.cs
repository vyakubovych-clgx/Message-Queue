using System.Runtime.Serialization;

namespace MessageQueue.Task2.Common.Infrastructure;

public static class BinarySerializer
{
    public static async Task<byte[]> SerializeAsync<T>(T @object)
    {
        await using var memoryStream = new MemoryStream();
        var serializer = new DataContractSerializer(typeof(T));
        serializer.WriteObject(memoryStream, @object);
        return memoryStream.ToArray();
    }

    public static async Task<T> DeserializeAsync<T>(byte[] bytes)
    {
        await using var memoryStream = new MemoryStream(bytes);
        var serializer = new DataContractSerializer(typeof(T));
        return (T)serializer.ReadObject(memoryStream);
    }
}
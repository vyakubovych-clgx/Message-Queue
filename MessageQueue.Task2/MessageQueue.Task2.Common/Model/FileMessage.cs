using System.Runtime.Serialization;

namespace MessageQueue.Task2.Common.Model;

[DataContract]
public record FileMessage
{
    [DataMember]
    public Guid FileId { get; init; }

    [DataMember] 
    public string FileName { get; init; }

    [DataMember]
    public byte[] Content { get; init; }

    [DataMember]
    public int ChunkNumber { get; init; }

    [DataMember]
    public int TotalChunksAmount { get; init; }
};
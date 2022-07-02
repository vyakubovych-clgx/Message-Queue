using System.Runtime.Serialization;
using MessageQueue.Task2.Common.Enums;

namespace MessageQueue.Task2.Common.Model;

[DataContract]
public record StatusMessage
{
    [DataMember]
    public Guid ServiceId { get; init; }

    [DataMember]
    public Status Status { get; init; }

    [DataMember]
    public int MaxMessageSize { get; init; }

    [DataMember]
    public int StatusSendingInterval { get; init; }
}
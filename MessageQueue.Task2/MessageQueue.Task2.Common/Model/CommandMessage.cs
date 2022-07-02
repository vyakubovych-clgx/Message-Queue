using System.Runtime.Serialization;
using MessageQueue.Task2.Common.Enums;

namespace MessageQueue.Task2.Common.Model;

[DataContract]
public record CommandMessage
{
    [DataMember]
    public Command Command { get; init; }

    [DataMember]
    public int? MaxMessageSize { get; init; }

    [DataMember]
    public int? StatusSendingInterval { get; init; }
}
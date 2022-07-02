using RabbitMQ.Client;

namespace MessageQueue.Task2.Common.RabbitMQ;

public class RabbitMQSender : RabbitMQWorker
{
    public RabbitMQSender(IConnection connection, string name, bool isExchange = false) : base(connection, name, isExchange)
    {
    }

    public void SendMessage(byte[] body)
    {
        using var channel = Connection.CreateModel();
        if (IsExchange)
            channel.BasicPublish(Name, string.Empty, null, body);
        else
            channel.BasicPublish(string.Empty, Name, null, body);
    }
}
using RabbitMQ.Client;

namespace MessageQueue.Task2.Common.RabbitMQ;

public abstract class RabbitMQWorker
{
    protected readonly IConnection Connection;
    protected readonly string Name;
    protected readonly bool IsExchange;

    protected RabbitMQWorker(IConnection connection, string name, bool isExchange = false)
    {
        Connection = connection;
        Name = name;
        IsExchange = isExchange;

        using var channel = Connection.CreateModel();
        if (IsExchange)
            channel.ExchangeDeclare(name, ExchangeType.Fanout);
        else
            channel.QueueDeclare(Name, false, false, false, null);
    }
}
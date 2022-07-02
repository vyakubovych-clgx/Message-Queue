using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MessageQueue.Task2.Common.RabbitMQ;

public class RabbitMQReceiver : RabbitMQWorker, IDisposable
{
    protected readonly IModel Channel;
    private bool _disposed;
    private readonly EventingBasicConsumer _consumer;
    private readonly string _tempQueueName;

    public RabbitMQReceiver(IConnection connection, string name, bool isExchange = false) : base(connection, name, isExchange)
    {
        Channel = Connection.CreateModel();

        if (IsExchange)
        {
            _tempQueueName = Channel.QueueDeclare().QueueName;
            Channel.QueueBind(_tempQueueName, Name, string.Empty);
        }

        _consumer = new EventingBasicConsumer(Channel);
    }

    public void SetReceivedMessageHandler(EventHandler<BasicDeliverEventArgs> onReceiveMessage)
    {
        _consumer.Received += onReceiveMessage;
    }

    public void StartReceivingMessages(bool autoAck)
    {
        Channel.BasicConsume(IsExchange ? _tempQueueName : Name, autoAck, _consumer);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }


    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
            Channel.Dispose();

        _disposed = true;
    }
}
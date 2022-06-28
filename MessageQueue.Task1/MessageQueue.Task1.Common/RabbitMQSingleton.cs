using MessageQueue.Task1.Common.Infrastructure;
using RabbitMQ.Client;

namespace MessageQueue.Task1.Common;

public abstract class RabbitMQSingleton<T> : IDisposable where T: RabbitMQSingleton<T>
{
    protected static readonly Lazy<T> Lazy =
        new(() => Activator.CreateInstance(typeof(T), true) as T);
    public static T Instance => Lazy.Value;

    protected readonly IConnection Connection;
    protected readonly string QueueName;
    private bool _disposed;

    protected RabbitMQSingleton()
    {
        var connectionFactory = new ConnectionFactory
        {
            HostName = ConfigHelper.GetMessageQueueEndpoint()
        };

        Connection = connectionFactory.CreateConnection();
        QueueName = ConfigHelper.GetQueueName();
    }

    public void QueueDeclare()
    {
        using var channel = Connection.CreateModel();
        channel.QueueDeclare(QueueName, false, false, false, null);
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
        {
            Connection.Dispose();
        }

        _disposed = true;
    }
}
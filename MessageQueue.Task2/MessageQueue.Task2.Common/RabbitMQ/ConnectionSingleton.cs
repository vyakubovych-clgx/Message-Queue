using MessageQueue.Task2.Common.Infrastructure;
using RabbitMQ.Client;

namespace MessageQueue.Task2.Common.RabbitMQ;

public class ConnectionSingleton : IDisposable
{
    private static readonly Lazy<ConnectionSingleton> Lazy = new(() => new ConnectionSingleton());
    private bool _disposed;

    public static ConnectionSingleton Instance => Lazy.Value;
    public IConnection Connection { get; }

    private ConnectionSingleton()
    {
        var connectionFactory = new ConnectionFactory
        {
            HostName = ConfigHelper.GetMessageQueueEndpoint()
        };

        Connection = connectionFactory.CreateConnection();
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
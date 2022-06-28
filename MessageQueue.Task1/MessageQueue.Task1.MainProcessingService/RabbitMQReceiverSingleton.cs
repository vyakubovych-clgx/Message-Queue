using MessageQueue.Task1.Common;
using MessageQueue.Task1.Common.Infrastructure;
using MessageQueue.Task1.Common.Model;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MessageQueue.Task1.MainProcessingService;

public class RabbitMQReceiverSingleton : RabbitMQSingleton<RabbitMQReceiverSingleton>
{
    private readonly EventingBasicConsumer _consumer;
    private readonly IModel _channel;
    private readonly Dictionary<Guid, List<(ulong, FileMessage)>> _receivedChunks = new();
    private bool _disposed;

    private RabbitMQReceiverSingleton()
    {
        _channel = Connection.CreateModel();
        _consumer = new EventingBasicConsumer(_channel);
        _consumer.Received += ReceivedMessage;
    }

    public void StartReceivingMessages()
    {
        _channel.BasicConsume(QueueName, false, _consumer);
    }

    private async void ReceivedMessage(object sender, BasicDeliverEventArgs eventArgs)
    {
        var body = eventArgs.Body.ToArray();
        var fileMessage = await BinarySerializer.DeserializeAsync<FileMessage>(body);
        var receivedMessage = (eventArgs.DeliveryTag, fileMessage);

        TryAddMessageToDictionary(fileMessage, receivedMessage);
        var currentFileChunks = _receivedChunks[fileMessage.FileId];

        if (ReceivedAllFileChunks(currentFileChunks, fileMessage))
        {
            await CreateNewFile(fileMessage.FileName, currentFileChunks);
            AcknowledgeChunkMessages(currentFileChunks);

            Console.WriteLine($"{fileMessage.FileName} was received.");
            _receivedChunks.Remove(fileMessage.FileId);
        }
    }

    private static bool ReceivedAllFileChunks(List<(ulong, FileMessage)> chunkMessages,
        FileMessage fileMessage)
        => chunkMessages.Count == fileMessage.TotalChunksAmount;


    private void TryAddMessageToDictionary(FileMessage fileMessage, (ulong, FileMessage) receivedMessage)
    {
        if (!_receivedChunks.TryAdd(fileMessage.FileId, new List<(ulong, FileMessage)> {receivedMessage}))
            _receivedChunks[fileMessage.FileId].Add(receivedMessage);
    }

    private void AcknowledgeChunkMessages(List<(ulong DeliveryTag, FileMessage)> chunkMessages)
    {
        foreach (var chunk in chunkMessages)
            _channel.BasicAck(chunk.DeliveryTag, false);
    }

    private static async Task CreateNewFile(string fileName, List<(ulong, FileMessage FileMessage)> chunkMessages)
    {
        var fileBytes = chunkMessages.OrderBy(c => c.FileMessage.ChunkNumber)
            .SelectMany(c => c.FileMessage.Content)
            .ToArray();

        var directoryWriter = new DirectoryWriter();
        await directoryWriter.CreateFileAsync(fileName, fileBytes);
    }

    protected override void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
            _channel.Dispose();

        _disposed = true;
        base.Dispose(disposing);
    }
}
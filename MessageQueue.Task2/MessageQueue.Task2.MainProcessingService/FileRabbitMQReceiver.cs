using MessageQueue.Task2.Common.Infrastructure;
using MessageQueue.Task2.Common.RabbitMQ;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MessageQueue.Task2.MainProcessingService;

public class FileRabbitMQReceiver : RabbitMQReceiver
{
    private readonly Dictionary<Guid, List<(ulong, Common.Model.FileMessage)>> _receivedChunks = new();
    private readonly DirectoryWriter _directoryWriter;

    public FileRabbitMQReceiver(IConnection connection, string name, DirectoryWriter directoryWriter) 
        : base(connection, name)
    {
        SetReceivedMessageHandler(ReceivedMessage);
        _directoryWriter = directoryWriter;
    }

    private async void ReceivedMessage(object sender, BasicDeliverEventArgs eventArgs)
    {
        var body = eventArgs.Body.ToArray();
        var fileMessage = await BinarySerializer.DeserializeAsync<Common.Model.FileMessage>(body).ConfigureAwait(false);
        var receivedMessage = (eventArgs.DeliveryTag, fileMessage);

        AddMessageToDictionary(fileMessage, receivedMessage);
        var currentFileChunks = _receivedChunks[fileMessage.FileId];

        if (ReceivedAllFileChunks(currentFileChunks, fileMessage))
        {
            await CreateNewFileAsync(fileMessage.FileName, currentFileChunks).ConfigureAwait(false);
            AcknowledgeChunkMessages(currentFileChunks);

            Console.WriteLine($"{fileMessage.FileName} was received.");
            _receivedChunks.Remove(fileMessage.FileId);
        }
    }

    private static bool ReceivedAllFileChunks(List<(ulong, Common.Model.FileMessage)> chunkMessages,
        Common.Model.FileMessage fileMessage)
        => chunkMessages.Count == fileMessage.TotalChunksAmount;


    private void AddMessageToDictionary(Common.Model.FileMessage fileMessage, (ulong, Common.Model.FileMessage) receivedMessage)
    {
        if (!_receivedChunks.TryAdd(fileMessage.FileId, new List<(ulong, Common.Model.FileMessage)> { receivedMessage }))
            _receivedChunks[fileMessage.FileId].Add(receivedMessage);
    }

    private void AcknowledgeChunkMessages(List<(ulong DeliveryTag, Common.Model.FileMessage)> chunkMessages)
    {
        foreach (var chunk in chunkMessages)
            Channel.BasicAck(chunk.DeliveryTag, false);
    }

    private async Task CreateNewFileAsync(string fileName, List<(ulong, Common.Model.FileMessage FileMessage)> chunkMessages)
    {
        var fileBytes = chunkMessages.OrderBy(c => c.FileMessage.ChunkNumber)
            .SelectMany(c => c.FileMessage.Content)
            .ToArray();

        await _directoryWriter.CreateFileAsync(fileName, fileBytes).ConfigureAwait(false);
    }
}
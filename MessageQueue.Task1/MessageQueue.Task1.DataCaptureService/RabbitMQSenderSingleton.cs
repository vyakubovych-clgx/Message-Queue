using MessageQueue.Task1.Common;
using MessageQueue.Task1.Common.Infrastructure;
using MessageQueue.Task1.Common.Model;
using RabbitMQ.Client;

namespace MessageQueue.Task1.DataCaptureService;

public class RabbitMQSenderSingleton : RabbitMQSingleton<RabbitMQSenderSingleton>
{
    private bool _disposed;

    public async Task SendFileMessageChunks(string fileName, byte[] fileContent)
    {
        var chunks = fileContent.Chunk(ConfigHelper.GetChunkSize()).ToList();
        var fileId = Guid.NewGuid();
        for (var i = 0; i < chunks.Count; i++)
        {
            var fileMessage = new FileMessage
            {
                FileId = fileId,
                FileName = fileName,
                Content = chunks[i],
                ChunkNumber = i,
                TotalChunksAmount = chunks.Count
            };
            var body = await BinarySerializer.SerializeAsync(fileMessage);
            using var channel = Connection.CreateModel();
            channel.BasicPublish(string.Empty, QueueName, null, body);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (_disposed) 
            return;
        _disposed = true;
        base.Dispose(disposing);
    }
}
using MessageQueue.Task2.Common.Enums;
using MessageQueue.Task2.Common.Infrastructure;
using MessageQueue.Task2.Common.Model;
using MessageQueue.Task2.Common.RabbitMQ;
using RabbitMQ.Client.Events;

namespace MessageQueue.Task2.DataCaptureService;

public class DataCaptureServiceWorker
{
    public Guid ServiceId { get; }
    public Status Status => _activeFileProcessingCount > 0 ? Status.ProcessingFile : Status.WaitingForFiles;

    private int _chunkSize = ConfigHelper.GetChunkSize();
    private int _statusSendingInterval = ConfigHelper.GetStatusSendingInterval();

    private readonly RabbitMQSender _fileMessageSender;
    private readonly RabbitMQSender _statusMessageSender;
    private int _activeFileProcessingCount;
    private Timer _statusSendingTimer;

    public DataCaptureServiceWorker(RabbitMQSender fileMessageSender, RabbitMQSender statusMessageSender)
    {
        ServiceId = Guid.NewGuid();
        _fileMessageSender = fileMessageSender;
        _statusMessageSender = statusMessageSender;
    }

    public void Work()
    {
        _statusSendingTimer = new Timer(async e => await SendStatusMessageAsync().ConfigureAwait(false), null,
            TimeSpan.Zero, TimeSpan.FromSeconds(_statusSendingInterval));

        Console.WriteLine("Press [enter] to exit.");
        Console.ReadLine();
    }

    public void SendFileMessage(FileSystemEventArgs eventArgs)
    {
        Task.Run(async () =>
        {
            Interlocked.Increment(ref _activeFileProcessingCount);
            var fileName = eventArgs.Name;
            for (var i = 0; i < 100; i++)
                try
                {
                    var fileContent = await File.ReadAllBytesAsync(eventArgs.FullPath).ConfigureAwait(false);
                    await SendFileMessageChunksAsync(fileName, fileContent).ConfigureAwait(false);
                    Console.WriteLine($"{fileName} was sent.");
                    Interlocked.Decrement(ref _activeFileProcessingCount);
                    return;
                }
                catch (IOException)
                {
                    await Task.Delay(100).ConfigureAwait(false);
                }

            Interlocked.Decrement(ref _activeFileProcessingCount);
            Console.WriteLine($"{fileName} - failed to send.");
        });
    }

    public async void HandleCommandMessage(object sender, BasicDeliverEventArgs eventArgs)
    {
        var messageBytes = eventArgs.Body.ToArray();
        var message = await BinarySerializer.DeserializeAsync<CommandMessage>(messageBytes).ConfigureAwait(false);
        switch (message.Command)
        {
            case Command.SendStatus:
                await SendStatusMessageAsync();
                break;
            case Command.ChangeSettings:
                HandleChangeSettingsCommand(message);
                break;
        }
    }

    private async Task SendFileMessageChunksAsync(string fileName, byte[] fileContent)
    {
        var chunks = fileContent.Chunk(_chunkSize).ToList();
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
            var body = await BinarySerializer.SerializeAsync(fileMessage).ConfigureAwait(false);
            _fileMessageSender.SendMessage(body);
        }
    }

    private async Task SendStatusMessageAsync()
    {
        var message = new StatusMessage
        {
            ServiceId = ServiceId,
            Status = Status,
            MaxMessageSize = _chunkSize,
            StatusSendingInterval = _statusSendingInterval
        };
        var messageBytes = await BinarySerializer.SerializeAsync(message).ConfigureAwait(false);
        _statusMessageSender.SendMessage(messageBytes);
        Console.WriteLine("Sent status message to Main Processing Service");
    }

    private void HandleChangeSettingsCommand(CommandMessage message)
    {
        if (message.MaxMessageSize is not null)
            _chunkSize = message.MaxMessageSize.Value;

        if (message.StatusSendingInterval is not null)
        {
            _statusSendingInterval = message.StatusSendingInterval.Value;
            _statusSendingTimer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(_statusSendingInterval));
        }
    }
}
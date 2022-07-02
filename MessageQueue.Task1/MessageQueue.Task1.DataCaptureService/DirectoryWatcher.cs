using MessageQueue.Task1.Common.Infrastructure;

namespace MessageQueue.Task1.DataCaptureService;

public class DirectoryWatcher : IDisposable
{
    private readonly FileSystemWatcher _fileSystemWatcher;
    private bool _disposed;

    public DirectoryWatcher()
    {
        var path = ConfigHelper.GetFileFolder();
        Directory.CreateDirectory(path);

        _fileSystemWatcher = new FileSystemWatcher(path)
        {
            EnableRaisingEvents = true,
            NotifyFilter = NotifyFilters.FileName
        };
    }

    public void StartWatching()
    {
        _fileSystemWatcher.Created += SendFile;
        _fileSystemWatcher.Renamed += SendFile;
        _fileSystemWatcher.Changed += SendFile;
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
            _fileSystemWatcher.Dispose();
        }

        _disposed = true;
    }

    private static void SendFile(object sender, FileSystemEventArgs eventArgs)
    {
        Task.Run(async () =>
        {
            var fileName = eventArgs.Name;
            for (var i = 0; i < 100; i++)
                try
                {
                    var fileContent = await File.ReadAllBytesAsync(eventArgs.FullPath).ConfigureAwait(false);
                    await RabbitMQSenderSingleton.Instance.SendFileMessageChunksAsync(fileName, fileContent)
                        .ConfigureAwait(false);
                    Console.WriteLine($"{fileName} was sent.");
                    return;
                }
                catch (IOException)
                {
                    await Task.Delay(100).ConfigureAwait(false);
                }

            Console.WriteLine($"{fileName} - failed to send.");
        });
    }
}
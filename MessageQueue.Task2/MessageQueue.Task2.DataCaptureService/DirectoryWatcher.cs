using MessageQueue.Task2.Common.Infrastructure;

namespace MessageQueue.Task2.DataCaptureService;

public class DirectoryWatcher : IDisposable
{
    private readonly FileSystemWatcher _fileSystemWatcher;
    private readonly DataCaptureServiceWorker _dataCaptureServiceWorker;
    private bool _disposed;

    public DirectoryWatcher(DataCaptureServiceWorker dataCaptureServiceWorker)
    {
        var path = ConfigHelper.GetFileFolder();
        Directory.CreateDirectory(path);

        _fileSystemWatcher = new FileSystemWatcher(path)
        {
            EnableRaisingEvents = true,
            NotifyFilter = NotifyFilters.FileName
        };

        _dataCaptureServiceWorker = dataCaptureServiceWorker;
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

    private void SendFile(object sender, FileSystemEventArgs eventArgs)
    {
        _dataCaptureServiceWorker.SendFileMessage(eventArgs);
    }
}
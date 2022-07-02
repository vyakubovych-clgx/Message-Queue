using MessageQueue.Task2.Common.Infrastructure;

namespace MessageQueue.Task2.MainProcessingService;

public class DirectoryWriter
{
    private readonly string _directory;
    public DirectoryWriter()
    {
        _directory = ConfigHelper.GetFileFolder();
        Directory.CreateDirectory(_directory);
    }

    public async Task CreateFileAsync(string fileName, byte[] fileBytes)
    {
        await using var writer = File.Create($"{_directory}/{fileName}");
        await writer.WriteAsync(fileBytes).ConfigureAwait(false);
    }
}
namespace MessageQueue.Task2.Common.Infrastructure;

public static class Constants
{
    public const string FilesQueueKey = "Files";
    public const string StatusesQueueKey = "Statuses";
    public const string CommandsQueueKey = "Commands";

    public const string StatusCommand = "status";
    public const string ConfigCommand = "config";
    public const string ExitCommand = "exit";
}
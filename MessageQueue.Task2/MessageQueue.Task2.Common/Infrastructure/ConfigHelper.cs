using Microsoft.Extensions.Configuration;

namespace MessageQueue.Task2.Common.Infrastructure;

public static class ConfigHelper
{
    private static readonly IConfigurationRoot Configuration =
        new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

    public static string GetMessageQueueEndpoint() => Configuration["MessageQueueEndpoint"];

    public static string GetQueueName(string queue) => Configuration.GetSection("Queues")[queue];

    public static string GetFileFolder() => Configuration["FileFolder"];

    public static int GetChunkSize() => int.Parse(Configuration["ChunkSize"]);

    public static int GetStatusSendingInterval() => int.Parse(Configuration["StatusSendingInterval"]);

}
using MessageQueue.Task2.Common.Enums;
using MessageQueue.Task2.Common.Infrastructure;
using MessageQueue.Task2.Common.Model;
using MessageQueue.Task2.Common.RabbitMQ;
using RabbitMQ.Client.Events;

namespace MessageQueue.Task2.MainProcessingService;

public class MainProcessingServiceWorker
{
    private readonly RabbitMQSender _commandSender;

    public MainProcessingServiceWorker(RabbitMQSender commandSender)
    {
        _commandSender = commandSender;
    }

    public async Task WorkAsync()
    {
        Console.WriteLine($"Enter \"{Constants.StatusCommand}\" to force all Data Capture Services to send their status.");
        Console.WriteLine($"Enter \"{Constants.ConfigCommand}\" to change parameters of Data Capture Services.");
        Console.WriteLine($"Enter \"{Constants.ExitCommand}\" to stop Main Processing Service.");
        while (true)
        {
            var command = Console.ReadLine();
            switch (command)
            {
                case Constants.StatusCommand:
                    await SendStatusCommandAsync();
                    break;
                case Constants.ConfigCommand:
                    await HandleConfigCommandAsync();
                    break;
                case Constants.ExitCommand:
                    Environment.Exit(0);
                    break;
            }

        }
    }

    public async void HandleStatusMessage(object sender, BasicDeliverEventArgs eventArgs)
    {
        var messageBytes = eventArgs.Body.ToArray();
        var message = await BinarySerializer.DeserializeAsync<StatusMessage>(messageBytes).ConfigureAwait(false);
        Console.WriteLine(message);
    }

    private async Task SendStatusCommandAsync()
    {
        var message = new CommandMessage
        {
            Command = Command.SendStatus
        };

        var messageBytes = await BinarySerializer.SerializeAsync(message).ConfigureAwait(false);
        _commandSender.SendMessage(messageBytes);
    }

    private async Task HandleConfigCommandAsync()
    {
        Console.WriteLine("Enter new maximum message size (in bytes), or empty string to not update it.");
        var maxMessageSize = ProcessUserInput();
        Console.WriteLine("Enter new status sending interval (in seconds), or empty string to not update it.");
        var statusSendingInterval = ProcessUserInput();

        var message = new CommandMessage
        {
            Command = Command.ChangeSettings,
            MaxMessageSize = maxMessageSize,
            StatusSendingInterval = statusSendingInterval
        };

        var messageBytes = await BinarySerializer.SerializeAsync(message).ConfigureAwait(false);
        _commandSender.SendMessage(messageBytes);

        Console.WriteLine("Change parameters command was sent successfully.");
    }

    private static int? ProcessUserInput()
    {
        while (true)
        {
            var input = Console.ReadLine();
            if (string.IsNullOrEmpty(input)) 
                return null;
            if (int.TryParse(input, out var value))
                return value;
            Console.WriteLine("Invalid input, try again.");
        }
    }
}
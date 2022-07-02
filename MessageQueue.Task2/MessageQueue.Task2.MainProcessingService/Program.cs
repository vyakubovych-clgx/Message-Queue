using MessageQueue.Task2.Common.Infrastructure;
using MessageQueue.Task2.Common.RabbitMQ;
using MessageQueue.Task2.MainProcessingService;

var directoryWriter = new DirectoryWriter();

using var connection = ConnectionSingleton.Instance;

var fileQueueName = ConfigHelper.GetQueueName(Constants.FilesQueueKey);
var fileReceiver = new FileRabbitMQReceiver(connection.Connection, fileQueueName, directoryWriter);
fileReceiver.StartReceivingMessages(false);

var commandsQueueName = ConfigHelper.GetQueueName(Constants.CommandsQueueKey);
var commandsSender = new RabbitMQSender(connection.Connection, commandsQueueName, true);

var worker = new MainProcessingServiceWorker(commandsSender);

var statusQueueName = ConfigHelper.GetQueueName(Constants.StatusesQueueKey);
var statusReceiver = new RabbitMQReceiver(connection.Connection, statusQueueName);
statusReceiver.SetReceivedMessageHandler(worker.HandleStatusMessage);
statusReceiver.StartReceivingMessages(true);

await worker.WorkAsync();
using MessageQueue.Task2.Common.Infrastructure;
using MessageQueue.Task2.Common.RabbitMQ;
using MessageQueue.Task2.DataCaptureService;

using var connection = ConnectionSingleton.Instance;

var fileQueueName = ConfigHelper.GetQueueName(Constants.FilesQueueKey);
var fileSender = new RabbitMQSender(connection.Connection, fileQueueName);

var statusQueueName = ConfigHelper.GetQueueName(Constants.StatusesQueueKey);
var statusSender = new RabbitMQSender(connection.Connection, statusQueueName);

var worker = new DataCaptureServiceWorker(fileSender, statusSender);

var commandsQueueName = ConfigHelper.GetQueueName(Constants.CommandsQueueKey);
var commandsReceiver = new RabbitMQReceiver(connection.Connection, commandsQueueName, true);
commandsReceiver.SetReceivedMessageHandler(worker.HandleCommandMessage);
commandsReceiver.StartReceivingMessages(true);

using var watcher = new DirectoryWatcher(worker);
watcher.StartWatching();

worker.Work();
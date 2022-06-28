using MessageQueue.Task1.DataCaptureService;

RabbitMQSenderSingleton.Instance.QueueDeclare(); 
using var watcher = new DirectoryWatcher();
watcher.StartWatching();

Console.WriteLine("Press [enter] to exit.");
Console.ReadLine();

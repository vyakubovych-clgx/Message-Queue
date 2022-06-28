using MessageQueue.Task1.MainProcessingService;

var receiver = RabbitMQReceiverSingleton.Instance;
receiver.QueueDeclare();
receiver.StartReceivingMessages();

Console.WriteLine("Press [enter] to exit.");
Console.ReadLine();
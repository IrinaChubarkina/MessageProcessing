namespace MessageProducer;

using System.Text;
using Configuration;
using RabbitMQ.Client;

public class RabbitMqPublisher : BackgroundService
{
    private readonly RabbitMqConfiguration _config;
    private IConnection _connection;
    private IModel _channel;

    public RabbitMqPublisher(RabbitMqConfiguration config)
    {
        _config = config;

        InitializeRabbitMq();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var random = new Random();
        var props = _channel.CreateBasicProperties();

        while (!stoppingToken.IsCancellationRequested)
        {
            var priority = random.Next(10);
            props.Priority = (byte)priority;

            var message = $"Message with priority {priority}";
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(
                exchange: string.Empty,
                routingKey: _config.Queue,
                basicProperties: props,
                body: body);

            Console.WriteLine("Sent: " + message);

            await Task.Delay(GetRandomInterval(), stoppingToken);
        }
    }
    
    private void InitializeRabbitMq()
    {
        var factory = new ConnectionFactory
                      {
                          HostName = _config.Host
                      };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        
        _channel.QueueDeclare(queue: _config.Queue,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object>
                       {
                           { "x-max-priority", 10 },
                       });
    }
    
    private TimeSpan GetRandomInterval()
    {
        return new TimeSpan(
            days: 0,
            hours:0,
            minutes:0,
            seconds: new Random().Next(60 * 60 * 5));
    }
    
    public override void Dispose()
    {
        if (_channel.IsOpen)
        {
            _channel.Close();
            _connection.Close();
        }
        
        base.Dispose();
    }
}

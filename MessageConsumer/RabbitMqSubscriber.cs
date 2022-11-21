namespace MessageConsumer;

using System.Text;
using Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public class RabbitMqSubscriber : BackgroundService
{
    private readonly RabbitMqConfiguration _config;
    private IConnection _connection;
    private IModel _channel;

    public RabbitMqSubscriber(RabbitMqConfiguration config)
    {
        _config = config;

        InitializeRabbitMq();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.Received += async (_, ea) =>
        {
            var message = Encoding.UTF8.GetString(ea.Body.ToArray());
            await ProcessMessage(message);
        };

        _channel.BasicConsume(queue: _config.Queue, autoAck: true, consumer: consumer);
        return Task.CompletedTask;
    }

    private void InitializeRabbitMq()
    {
        var factory = new ConnectionFactory
                      {
                          HostName = _config.Host,
                          DispatchConsumersAsync = true
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
    
    private async Task ProcessMessage(string message)
    {
        Console.WriteLine($"Processed: {message}");

        // Simulate slow connection
        await Task.Delay(1000);
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

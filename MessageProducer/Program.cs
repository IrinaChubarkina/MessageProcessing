using MessageProducer;
using MessageProducer.Configuration;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var appSettings = context.Configuration.Get<AppSettings>();

        services
            .AddSingleton(appSettings.RabbitMq)
            .AddHostedService<RabbitMqPublisher>();
    })
    .Build();

host.Run();

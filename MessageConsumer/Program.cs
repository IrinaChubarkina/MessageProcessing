using MessageConsumer;
using MessageConsumer.Configuration;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var appSettings = context.Configuration.Get<AppSettings>();

        services
            .AddSingleton(appSettings.RabbitMq)
            .AddHostedService<RabbitMqSubscriber>();
    })
    .Build();

host.Run();

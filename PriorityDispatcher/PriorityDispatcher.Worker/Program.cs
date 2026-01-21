using PriorityDispatcher.Contracts.Interfaces;
using PriorityDispatcher.Services.Encryption;
using PriorityDispatcher.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<IEncryptionService, AesEncryptionService>();

var host = builder.Build();
host.Run();

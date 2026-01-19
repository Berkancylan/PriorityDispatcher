using PriorityDispatcher.Contracts.Interfaces;
using PriorityDispatcher.Services;
using PriorityDispatcher.Services.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddSingleton<IEncryptionService,AesEncryptionService>();
builder.Services.AddSingleton<INotificationQueueService, NotificationQueueService>();
builder.Services.AddHostedService<NotificationWorker>();



// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

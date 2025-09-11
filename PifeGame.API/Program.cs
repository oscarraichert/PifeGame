using Microsoft.AspNetCore.Builder;
using PifeGame.API;
using PifeGame.Application;
using PifeGame.Domain;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
builder.Services.AddScoped<GameHub>();
builder.Services.AddScoped<GameHandler>();
builder.Services.AddSingleton<GameHub>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.UseSwagger();
    //app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseWebSockets();

app.Use(async (context, next) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var handler = new WebSocketHandler();
        await handler.HandleAsync(webSocket);
    }
    else
    {
        await next(context);
    }
});

app.MapGet("/deck-test", () =>
{
    return new Game().Deck;
});

app.MapGet("/list-rooms", (GameHub hub) =>
{
    return hub.ListRooms();
});

app.Run();

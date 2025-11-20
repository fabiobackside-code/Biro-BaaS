using System.Collections.Concurrent;
using Biro.Blocks.Authentication;
using bks.sdk.Core.Initialization;
using bks.sdk.Security.JWT;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBKSFramework(builder.Configuration);
builder.Services.AddSingleton<InMemoryUserStore>();

var app = builder.Build();

app.UseBKSFramework();

app.MapPost("/register", async (User user, InMemoryUserStore store) =>
{
    store.Users.TryAdd(user.Username, user);
    return Results.Ok();
});

app.MapPost("/login", async ([FromBody] UserCredentials credentials, InMemoryUserStore store, IJwtProvider jwtProvider) =>
{
    if (store.Users.TryGetValue(credentials.Username, out var user) && user.Password == credentials.Password)
    {
        var token = jwtProvider.GenerateToken(user.Username, new Dictionary<string, string> { { "sub", user.Username } });
        return Results.Ok(new { Token = token });
    }

    return Results.Unauthorized();
});

app.Run();

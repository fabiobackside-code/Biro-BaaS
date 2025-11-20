using Biro.Core.Application.Repositories;
using Biro.Infrastructure.Persistence.Dapper;
using bks.sdk.Core.Initialization;
using Dapper;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBKSFramework(builder.Configuration);
builder.Services.AddSingleton<IDbConnectionFactory>(_ => new DbConnectionFactory(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IAccountRepository, AccountRepository>();

var app = builder.Build();

app.UseBKSFramework();

app.MapGet("/balance/{accountId}", async (Guid accountId, IDbConnectionFactory connectionFactory) =>
{
    using var connection = await connectionFactory.CreateConnectionAsync();
    const string sql = "SELECT get_balance(@accountId)";
    var balance = await connection.ExecuteScalarAsync<decimal>(sql, new { accountId });
    return Results.Ok(new { AccountId = accountId, Balance = balance });
});

app.Run();

using System.Net;
using System.Net.Http.Json;
using Biro.Blocks.Debit;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Biro.Tests.Integration;

public class DebitServiceTests : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly IContainer _postgresContainer;
    private readonly IContainer _rabbitMqContainer;

    public DebitServiceTests()
    {
        _postgresContainer = new ContainerBuilder()
            .WithImage("postgres:15")
            .WithEnvironment("POSTGRES_PASSWORD", "password")
            .WithPortBinding(5432, true)
            .Build();
        _rabbitMqContainer = new ContainerBuilder()
            .WithImage("rabbitmq:3-management")
            .WithPortBinding(5672, true)
            .Build();

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, conf) =>
            {
                conf.AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "ConnectionStrings:DefaultConnection", _postgresContainer.ConnectionString },
                    { "ConnectionStrings:RabbitMQ", _rabbitMqContainer.ConnectionString }
                });
            });
        });
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
        await _rabbitMqContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgresContainer.StopAsync();
        await _rabbitMqContainer.StopAsync();
    }

    [Fact]
    public async Task Post_Debit_ShouldReturnAccepted()
    {
        // Arrange
        var client = _factory.CreateClient();
        var debitRequest = new DebitRequest { AccountId = Guid.NewGuid(), Amount = 100 };

        // Act
        var response = await client.PostAsJsonAsync("/debit", debitRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var responseBody = await response.Content.ReadFromJsonAsync<object>();
        responseBody.Should().NotBeNull();
        responseBody.ToString().Should().Contain("TransactionId");
    }
}

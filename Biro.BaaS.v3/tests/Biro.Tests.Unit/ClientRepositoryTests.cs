using System.Data;
using Biro.Core.Application.Repositories;
using Biro.Core.Domain.Entities;
using Biro.Infrastructure.Persistence.Dapper;
using Biro.Infrastructure.Persistence.Dapper.Repositories;
using Dapper;
using FluentAssertions;
using Moq;
using Xunit;

namespace Biro.Tests.Unit;

public class ClientRepositoryTests
{
    private readonly Mock<IDbConnectionFactory> _mockConnectionFactory;
    private readonly Mock<IDbConnection> _mockConnection;
    private readonly IClientRepository _clientRepository;

    public ClientRepositoryTests()
    {
        _mockConnectionFactory = new Mock<IDbConnectionFactory>();
        _mockConnection = new Mock<IDbConnection>();
        _mockConnectionFactory.Setup(x => x.CreateConnectionAsync()).ReturnsAsync(_mockConnection.Object);
        _clientRepository = new ClientRepository(_mockConnectionFactory.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnClient_WhenClientExists()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var client = new Client { Id = clientId, Name = "Test Client", Document = "123456789" };
        _mockConnection.Setup(db => db.QueryFirstOrDefaultAsync<Client>(It.IsAny<string>(), It.IsAny<object>(), null, null, null)).ReturnsAsync(client);

        // Act
        var result = await _clientRepository.GetByIdAsync(clientId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(clientId);
    }

    [Fact]
    public async Task AddAsync_ShouldExecuteInsertStatement()
    {
        // Arrange
        var client = new Client { Id = Guid.NewGuid(), Name = "Test Client", Document = "123456789" };

        // Act
        await _clientRepository.AddAsync(client);

        // Assert
        _mockConnection.Verify(db => db.ExecuteAsync(It.IsAny<string>(), It.IsAny<object>(), null, null, null), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldExecuteUpdateStatement()
    {
        // Arrange
        var client = new Client { Id = Guid.NewGuid(), Name = "Test Client", Document = "123456789" };

        // Act
        await _clientRepository.UpdateAsync(client);

        // Assert
        _mockConnection.Verify(db => db.ExecuteAsync(It.IsAny<string>(), It.IsAny<object>(), null, null, null), Times.Once);
    }
}

using Biro.Core.Domain.Entities;

namespace Biro.Core.Application.Repositories;

public interface IClientRepository
{
    Task<Client> GetByIdAsync(Guid id);
    Task AddAsync(Client client);
    Task UpdateAsync(Client client);
}

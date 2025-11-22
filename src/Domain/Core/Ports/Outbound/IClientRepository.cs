using Domain.Core.Entities;
using System.Threading.Tasks;

namespace Domain.Core.Ports.Outbound
{
    public interface IClientRepository
    {
        Task CreateClientAsync(Client client);
        Task<Client> GetClientByIdAsync(Guid clientId);
        Task UpdateClientAsync(Client client);
    }
}

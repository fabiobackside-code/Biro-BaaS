using Domain.Core.Entities;
using System.Threading.Tasks;

namespace Domain.Core.Ports.Inbound
{
    public interface IClientUseCases
    {
        Task CreateClientAsync(Client client);
        Task<Client> GetClientByIdAsync(Guid clientId);
    }
}

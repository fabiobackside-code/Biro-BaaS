using Domain.Core.Entities;
using Domain.Core.Ports.Inbound;
using Domain.Core.Ports.Outbound;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class ClientUseCases : IClientUseCases
    {
        private readonly IUnitOfWork _unitOfWork;

        public ClientUseCases(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task CreateClientAsync(Client client)
        {
            await _unitOfWork.Clients.CreateClientAsync(client);
        }

        public async Task<Client> GetClientByIdAsync(System.Guid clientId)
        {
            return await _unitOfWork.Clients.GetClientByIdAsync(clientId);
        }
    }
}

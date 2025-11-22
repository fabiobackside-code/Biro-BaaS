using Application.DTOs;
using Domain.Core.Entities;
using Domain.Core.Ports.Inbound;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Adapters.Inbound.REST.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ClientsController : ControllerBase
    {
        private readonly IClientUseCases _clientUseCases;

        public ClientsController(IClientUseCases clientUseCases)
        {
            _clientUseCases = clientUseCases;
        }

        [HttpPost]
        public async Task<IActionResult> CreateClient([FromBody] CreateClientRequest request)
        {
            var client = new Client
            {
                ClientId = Guid.NewGuid(),
                TaxId = request.TaxId,
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                DateOfBirth = request.DateOfBirth,
                Status = "ACTIVE",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _clientUseCases.CreateClientAsync(client);

            var response = new ClientDetailsResponse
            {
                ClientId = client.ClientId,
                TaxId = client.TaxId,
                FullName = client.FullName,
                Email = client.Email,
                Phone = client.Phone,
                DateOfBirth = client.DateOfBirth,
                Status = client.Status,
                CreatedAt = client.CreatedAt
            };

            return CreatedAtAction(nameof(GetClientDetails), new { clientId = client.ClientId }, response);
        }

        [HttpGet("{clientId}")]
        public async Task<IActionResult> GetClientDetails(Guid clientId)
        {
            var client = await _clientUseCases.GetClientByIdAsync(clientId);
            if (client == null)
            {
                return NotFound();
            }

            var response = new ClientDetailsResponse
            {
                ClientId = client.ClientId,
                TaxId = client.TaxId,
                FullName = client.FullName,
                Email = client.Email,
                Phone = client.Phone,
                DateOfBirth = client.DateOfBirth,
                Status = client.Status,
                CreatedAt = client.CreatedAt
            };

            return Ok(response);
        }
    }
}

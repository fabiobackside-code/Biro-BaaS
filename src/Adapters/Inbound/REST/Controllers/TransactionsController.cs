using Application.DTOs;
using Domain.Core.Ports.Inbound;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Adapters.Inbound.REST.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionUseCases _transactionUseCases;

        public TransactionsController(ITransactionUseCases transactionUseCases)
        {
            _transactionUseCases = transactionUseCases;
        }

        [HttpPost("debit")]
        public async Task<IActionResult> Debit([FromBody] DebitRequest request)
        {
            await _transactionUseCases.DebitAsync(request.AccountId, request.Amount);
            return Ok();
        }

        [HttpPost("credit")]
        public async Task<IActionResult> Credit([FromBody] CreditRequest request)
        {
            await _transactionUseCases.CreditAsync(request.AccountId, request.Amount);
            return Ok();
        }

        [HttpPost("transfer")]
        public async Task<IActionResult> Transfer([FromBody] TransferRequest request)
        {
            await _transactionUseCases.TransferAsync(request.SourceAccountId, request.DestinationAccountId, request.Amount);
            return Ok();
        }

        [HttpPost("block")]
        public async Task<IActionResult> Block([FromBody] BlockRequest request)
        {
            await _transactionUseCases.BlockAsync(request.AccountId, request.Amount);
            return Ok();
        }

        [HttpPost("unblock")]
        public async Task<IActionResult> Unblock([FromBody] UnblockRequest request)
        {
            await _transactionUseCases.UnblockAsync(request.TransactionId);
            return Ok();
        }

        [HttpPost("reservation")]
        public async Task<IActionResult> Reservation([FromBody] ReservationRequest request)
        {
            await _transactionUseCases.ReservationAsync(request.AccountId, request.Amount);
            return Ok();
        }

        [HttpPost("settle")]
        public async Task<IActionResult> Settle([FromBody] SettleRequest request)
        {
            await _transactionUseCases.SettleAsync(request.TransactionId);
            return Ok();
        }
    }
}

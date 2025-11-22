using Application.DTOs;
using Domain.Core.Entities;
using Domain.Core.Ports.Inbound;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Adapters.Inbound.REST.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountUseCases _accountUseCases;

        public AccountsController(IAccountUseCases accountUseCases)
        {
            _accountUseCases = accountUseCases;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest request)
        {
            var account = new Account
            {
                AccountId = Guid.NewGuid(),
                ClientId = request.ClientId,
                ProductType = request.ProductType,
                BranchCode = request.BranchCode,
                AccountNumber = request.AccountNumber,
                Status = "ACTIVE",
                OpenedAt = DateTime.UtcNow
            };
            await _accountUseCases.CreateAccountAsync(account);

            var response = new AccountDetailsResponse
            {
                AccountId = account.AccountId,
                ClientId = account.ClientId,
                ProductType = account.ProductType,
                BranchCode = account.BranchCode,
                AccountNumber = account.AccountNumber,
                Status = account.Status,
                OpenedAt = account.OpenedAt
            };

            return CreatedAtAction(nameof(GetAccountDetails), new { accountId = account.AccountId }, response);
        }

        [HttpGet("{accountId}")]
        public async Task<IActionResult> GetAccountDetails(Guid accountId)
        {
            var account = await _accountUseCases.GetAccountByIdAsync(accountId);
            if (account == null)
            {
                return NotFound();
            }

            var response = new AccountDetailsResponse
            {
                AccountId = account.AccountId,
                ClientId = account.ClientId,
                ProductType = account.ProductType,
                BranchCode = account.BranchCode,
                AccountNumber = account.AccountNumber,
                Status = account.Status,
                OpenedAt = account.OpenedAt
            };

            return Ok(response);
        }

        [HttpGet("{accountId}/balance")]
        public async Task<IActionResult> GetBalance(Guid accountId)
        {
            var balance = await _accountUseCases.GetAccountBalanceAsync(accountId);
            return Ok(new { AccountId = accountId, Balance = balance });
        }

        [HttpGet("{accountId}/statement")]
        public async Task<IActionResult> GetStatement(Guid accountId)
        {
            var statement = await _accountUseCases.GetAccountStatementAsync(accountId);
            var response = statement.Select(t => new TransactionResponse
            {
                TransactionId = t.TransactionId,
                AccountId = t.AccountId,
                TransactionType = t.TransactionType,
                Amount = t.Amount,
                Timestamp = t.Timestamp,
                Status = t.Status
            });
            return Ok(response);
        }
    }
}

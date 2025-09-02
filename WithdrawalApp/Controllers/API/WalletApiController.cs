using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WithdrawalApp.Interfaces;
using WithdrawalApp.Models;

namespace WithdrawalApp.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class WalletApiController : ControllerBase
    {
        private readonly IWalletService _walletService;
        private readonly IAntiforgery _antiforgery;

        public WalletApiController(IWalletService walletService, IAntiforgery antiforgery)
        {
            _walletService = walletService;
            _antiforgery = antiforgery;
        }

        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance()
        {
            var bal = await _walletService.GetBalanceAsync();
            return Ok(bal);
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactions()
        {
            var txs = await _walletService.GetTransactionsAsync();
            return Ok(txs);
        }

        [HttpPost("cancel")]
        [ValidateAntiForgeryToken]
        public IActionResult Cancel([FromBody] CancelRequest req)
        {
            if (req == null || req.TransactionId == Guid.Empty) return BadRequest();
            var ok = _walletService.CancelTransactionAsync(req.TransactionId);
            return Ok(new { success = ok });
        }
    }
}

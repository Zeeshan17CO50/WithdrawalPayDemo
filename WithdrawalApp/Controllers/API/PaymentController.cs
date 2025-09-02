using Microsoft.AspNetCore.Mvc;
using WithdrawalApp.Interfaces;
using WithdrawalApp.Request;

namespace WithdrawalApp.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IWalletService _walletService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IWalletService walletService, ILogger<PaymentController> logger)
        {
            _walletService = walletService;
            _logger = logger;
        }

        [HttpPost("callback")]
        public async Task<IActionResult> Callback(CallbackRequest req)
        {
            if (req == null || req.TransactionId == Guid.Empty || string.IsNullOrEmpty(req.Status))
                return BadRequest();

            var processed = await _walletService.ProcessCallbackAsync(req.TransactionId, req.Status, req.GatewayReference);
            //if (processed) return Ok();
            //return StatusCode(500);
            return Ok(new { success = processed });
        }
    }
}

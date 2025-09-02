using Microsoft.AspNetCore.Mvc;
using WithdrawalApp.Interfaces;

namespace WithdrawalApp.Controllers
{
    public class WalletController : Controller
    {
        private readonly IWalletService _walletService;
        private readonly IHttpClientFactory _httpFactory;
        private readonly IConfiguration _config;
        private readonly ILogger<WalletController> _logger;

        public WalletController(IWalletService walletService, IHttpClientFactory httpFactory, 
            IConfiguration config, ILogger<WalletController> logger)
        {
            _walletService = walletService;
            _httpFactory = httpFactory;
            _config = config;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Balance = await _walletService.GetBalanceAsync();
            var txs = await _walletService.GetTransactionsAsync();
            return View(txs);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Withdraw(decimal amount, string method)
        {
            if (amount <= 0)
            {
                TempData["Error"] = "Amount must be positive.";
                return RedirectToAction(nameof(Index));
            }

            var currentBalance = await _walletService.GetBalanceAsync();
            if (amount > currentBalance)
            {
                TempData["Error"] = "Insufficient balance at request time.";
                return RedirectToAction(nameof(Index));
            }

            var tx = await _walletService.CreateTransactionAsync(amount, method);

            // SImulatore Base Url Request from config
            var simBase = _config["PaymentSimulator:BaseUrl"]?.TrimEnd('/');
            var client = _httpFactory.CreateClient();

            var callbackUrl = $"{Request.Scheme}://{Request.Host}/api/Payment/callback";
            var processReq = new
            {
                transactionId = tx.Id,
                amount = tx.Amount,
                method = tx.PaymentMethod,
                callbackUrl = callbackUrl
            };

            try
            {
                var resp = await client.PostAsJsonAsync($"{simBase}/api/payment/acknowledge", processReq);
                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogWarning("PaymentSimulator {Code}", resp.StatusCode);
                    TempData["Error"] = "Payment simulator error. Transaction is Pending.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment simulator unreachable. Transaction is Pending.");
                TempData["Error"] = "Payment simulator unreachable. Transaction is Pending.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(Guid id)
        {
            var ok = await _walletService.CancelTransactionAsync(id);
            if (!ok) TempData["Error"] = "Unable to cancel Transaction (must be Pending or Success).";
            return RedirectToAction(nameof(Index));
        }
    }
}

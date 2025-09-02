using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PaymentSimulator.Models;
using System;
using System.Collections.Concurrent;

namespace PaymentSimulator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IHttpClientFactory _httpFactory;
        private static readonly ConcurrentDictionary<string, (Guid TxId, string CallbackUrl)> _store = new();
        private readonly ILogger<PaymentController> _logger;
        private readonly Random _rnd = new();

        private const double SuccessProbability = 0.6; 

        public PaymentController(IHttpClientFactory httpFactory, ILogger<PaymentController> logger)
        {
            _httpFactory = httpFactory;
            _logger = logger;
        }

        [HttpPost("acknowledge")]
        public IActionResult Process([FromBody] ProcessRequest req)
        {
            if (req == null || req.TransactionId == Guid.Empty || string.IsNullOrWhiteSpace(req.CallbackUrl))
                return BadRequest(new { error = "transactionId and callbackUrl required" });

            // Simulate async gateway processing
            _ = Task.Run(async () =>
            {
                try
                {
                    var delay = _rnd.Next(1000, 3000);
                    await Task.Delay(delay);

                    var success = _rnd.NextDouble() < SuccessProbability;
                    var status = success ? "success" : "failed";
                    var gatewayRef = $"GATEWAY-{Guid.NewGuid().ToString().Split('-')[0].ToUpper()}";

                    var payload = new
                    {
                        transactionId = req.TransactionId,
                        status = status,
                        gatewayReference = gatewayRef
                    };

                    var client = _httpFactory.CreateClient();
                    client.Timeout = TimeSpan.FromSeconds(10);

                    _logger.LogInformation("Calling callback {Url} for Tx {Tx} -> {Status}", req.CallbackUrl, req.TransactionId, status);

                    try
                    {
                        var resp = await client.PostAsJsonAsync(req.CallbackUrl, payload);
                        _logger.LogInformation("Callback response {Status} for Tx {Tx}", resp.StatusCode, req.TransactionId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to call callback for Tx {Tx}", req.TransactionId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Simulator internal error");
                }
            });

            return Accepted(new { message = "Processing started", transactionId = req.TransactionId });
        }


        //[HttpPost("callback")]
        //public async Task<IActionResult> ManualCallback([FromBody] ManualCallbackRequest req)
        //{
        //    if (req == null || string.IsNullOrWhiteSpace(req.GatewayRef) || string.IsNullOrWhiteSpace(req.Status))
        //        return BadRequest();

        //    if (!_store.TryGetValue(req.GatewayRef, out var info))
        //        return NotFound(new { error = "gatewayRef not found" });

        //    var payload = new
        //    {
        //        transactionId = info.TxId,
        //        status = req.Status,
        //        gatewayReference = req.GatewayRef
        //    };

        //    var client = _httpFactory.CreateClient();
        //    try
        //    {
        //        var resp = await client.PostAsJsonAsync(info.CallbackUrl, payload);
        //        return Ok(new { result = "callback_sent", statusCode = resp.StatusCode });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Manual callback failed for {GatewayRef}", req.GatewayRef);
        //        return StatusCode(500, new { error = "callback failed", detail = ex.Message });
        //    }
        //}

    }
}

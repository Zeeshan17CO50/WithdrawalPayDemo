namespace PaymentSimulator.Models
{
    public class ManualCallbackRequest
    {
        public string GatewayRef { get; set; } = "";
        public string Status { get; set; } = ""; // "success" or "failed"
    }
}

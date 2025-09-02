namespace PaymentSimulator.Models
{
    public class ProcessRequest
    {
        public Guid TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string Method { get; set; } = "";
        public string CallbackUrl { get; set; } = "https://localhost:7256/api/Payment/callback";
    }
}

namespace PaymentSimulator.Models
{
    public class PaymentAckRequest
    {
        public Guid TransactionId { get; set; } // WalletApp transaction id
        public decimal Amount { get; set; }
        public string Method { get; set; } = "";
        public string CallbackUrl { get; set; } = ""; // WalletApp webhook url
    }
}

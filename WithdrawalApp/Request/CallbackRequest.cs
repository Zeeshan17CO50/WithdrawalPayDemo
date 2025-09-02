namespace WithdrawalApp.Request
{
    public class CallbackRequest
    {
        public Guid TransactionId { get; set; }
        public string Status { get; set; } 
        public string? GatewayReference { get; set; }
    }
}

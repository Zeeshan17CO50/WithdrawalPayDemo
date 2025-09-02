namespace PaymentSimulator.Models
{
    public class PaymentAckResponse
    {
        public string GatewayRef { get; set; } = "";
        public string Status { get; set; } = "pending";
    }
}

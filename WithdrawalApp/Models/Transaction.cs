using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Transactions;

namespace WithdrawalApp.Models
{
    public enum TransactionStatus
    {
        Pending = 0,
        Success = 1,
        Failed = 2,
        Canceled = 3
    }
    public class Transaction
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required, MaxLength(50)]
        public string PaymentMethod { get; set; }

        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,2)")]
        public decimal WalletBalanceAfter { get; set; }

        [MaxLength(200)]
        public string? GatewayReference { get; set; }
    }
}

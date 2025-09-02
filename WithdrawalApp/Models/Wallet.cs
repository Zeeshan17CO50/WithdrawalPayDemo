using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WithdrawalApp.Models
{
    public class Wallet
    {
        [Key]
        public int Id { get; set; } = 1;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }

    public class CancelRequest { 
        public Guid TransactionId { get; set; } 
    }

}

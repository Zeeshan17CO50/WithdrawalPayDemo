using Microsoft.EntityFrameworkCore;
using WithdrawalApp.Data;
using WithdrawalApp.Interfaces;
using WithdrawalApp.Models;

namespace WithdrawalApp.Services
{
    public class WalletService: IWalletService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<WalletService> _logger;

        public WalletService(AppDbContext db, ILogger<WalletService> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// Get Balance
        /// </summary>
        /// <returns></returns>
        public async Task<decimal> GetBalanceAsync()
        {
            var w = await _db.Wallets.FindAsync(1);
            return w?.Balance ?? 0m;
        }

        /// <summary>
        /// Get History
        /// </summary>
        /// <returns></returns>
        public async Task<List<Transaction>> GetTransactionsAsync()
        {
            return await _db.Transactions.OrderByDescending(t => t.Timestamp).ToListAsync();
        }

        /// <summary>
        /// Create payment transaction
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="paymentMethod"></param>
        /// <returns></returns>
        public async Task<Transaction> CreateTransactionAsync(decimal amount, string paymentMethod)
        {
            // Check first if wallet exists else add initial amount
            var wallet = await _db.Wallets.FindAsync(1);
            if (wallet == null)
            {
                wallet = new Wallet { Id = 1, Balance = 1000m };
                _db.Wallets.Add(wallet);
                await _db.SaveChangesAsync();
            }

            var tx = new Transaction
            {
                Amount = amount,
                PaymentMethod = paymentMethod,
                Status = TransactionStatus.Pending,
                Timestamp = DateTime.UtcNow,
                WalletBalanceAfter = wallet.Balance
            };

            _db.Transactions.Add(tx);
            await _db.SaveChangesAsync();

            return tx;
        }

        /// <summary>
        /// Cancel
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> CancelTransactionAsync(Guid id)
        {
            using var dbTx = await _db.Database.BeginTransactionAsync();
            try
            {
                var tx = await _db.Transactions.FirstOrDefaultAsync(t => t.Id == id);
                if (tx == null) return false;

                var wallet = await _db.Wallets.FirstOrDefaultAsync(w => w.Id == 1);
                if (wallet == null) return false;

                if (tx.Status == TransactionStatus.Pending)
                {
                    tx.Status = TransactionStatus.Canceled;
                    tx.WalletBalanceAfter = wallet.Balance;
                }
                else if (tx.Status == TransactionStatus.Success)
                {
                    // Refund amount
                    wallet.Balance += tx.Amount;
                    tx.Status = TransactionStatus.Canceled;
                    tx.WalletBalanceAfter = wallet.Balance;
                }
                else
                {
                    return false;
                }

                _db.Wallets.Update(wallet);
                _db.Transactions.Update(tx);
                await _db.SaveChangesAsync();
                await dbTx.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling transaction {Tx}", id);
                await dbTx.RollbackAsync();
                return false;
            }
        }

        /// <summary>
        /// Generate call back
        /// </summary>
        /// <param name="transactionId"></param>
        /// <param name="status"></param>
        /// <param name="gatewayReference"></param>
        /// <returns></returns>
        public async Task<bool> ProcessCallbackAsync(Guid transactionId, string status, string? gatewayReference)
        {
            using var dbTx = await _db.Database.BeginTransactionAsync();
            try
            {
                var tx = await _db.Transactions.FirstOrDefaultAsync(t => t.Id == transactionId);
                if (tx == null)
                {
                    _logger.LogWarning("Callback for transaction {Tx}", transactionId);
                    return false;
                }

                if (tx.Status != TransactionStatus.Pending)
                {
                    _logger.LogInformation("Callback ignored for {Tx} because status is {Status}", tx.Id, tx.Status);
                    return true;
                }

                var wallet = await _db.Wallets.FirstOrDefaultAsync(w => w.Id == 1);
                if (wallet == null)
                {
                    _logger.LogError("Wallet missing during callback");
                    return false;
                }

                // save gateway ref when callback arrives
                tx.GatewayReference = gatewayReference;

                // Amount deducted from Balance after successful payment

                if (string.Equals(status, "success", StringComparison.OrdinalIgnoreCase))
                {
                    if (wallet.Balance >= tx.Amount)
                    {
                        wallet.Balance -= tx.Amount;
                        tx.Status = TransactionStatus.Success;
                    }
                    else
                    {
                        // insufficient Balance processing time
                        tx.Status = TransactionStatus.Failed;
                    }
                }
                else
                {
                    tx.Status = TransactionStatus.Failed;
                }

                tx.WalletBalanceAfter = wallet.Balance;

                _db.Wallets.Update(wallet);
                _db.Transactions.Update(tx);
                await _db.SaveChangesAsync();
                await dbTx.CommitAsync();

                _logger.LogInformation("Processed callback for {Tx} - {Status}", tx.Id, tx.Status);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing callback for {Tx}", transactionId);
                await dbTx.RollbackAsync();
                return false;
            }
        }
    }
}

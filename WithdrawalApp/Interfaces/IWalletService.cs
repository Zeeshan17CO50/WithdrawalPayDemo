using WithdrawalApp.Models;

namespace WithdrawalApp.Interfaces
{
    public interface IWalletService
    {
        Task<decimal> GetBalanceAsync();
        Task<List<Transaction>> GetTransactionsAsync();
        Task<Transaction> CreateTransactionAsync(decimal amount, string paymentMethod);
        Task<bool> CancelTransactionAsync(Guid id);
        Task<bool> ProcessCallbackAsync(Guid transactionId, string status, string? gatewayReference);
    }
}

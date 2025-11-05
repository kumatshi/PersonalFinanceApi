using PersonalFinanceApi.Models;

namespace PersonalFinanceApi.Interfaces
{
    public interface ITransactionRepository : IRepository<Transaction>
    {
        Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Transaction>> GetByAccountIdAsync(int accountId);
        Task<IEnumerable<Transaction>> GetByCategoryIdAsync(int categoryId);
        Task<IEnumerable<Transaction>> GetByTypeAsync(TransactionType type);
        Task<decimal> GetTotalIncomeAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<decimal> GetTotalExpensesAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<IEnumerable<Transaction>> GetRecentTransactionsAsync(int count = 10);
        Task<IEnumerable<Transaction>> GetTransactionsWithDetailsAsync();
    }
}
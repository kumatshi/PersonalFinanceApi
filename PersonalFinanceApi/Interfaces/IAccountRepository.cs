using PersonalFinanceApi.Models;

namespace PersonalFinanceApi.Interfaces
{
    public interface IAccountRepository : IRepository<Account>
    {
        Task<IEnumerable<Account>> GetByTypeAsync(AccountType type);
        Task<Account> GetByNameAsync(string name);
        Task<decimal> GetTotalBalanceAsync();
        Task<bool> AccountHasTransactionsAsync(int accountId);
        Task UpdateAccountBalanceAsync(int accountId, decimal amount, TransactionType transactionType);
    }
}
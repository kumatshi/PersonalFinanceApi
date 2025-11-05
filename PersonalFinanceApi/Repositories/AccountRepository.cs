using Microsoft.EntityFrameworkCore;
using PersonalFinanceApi.Data;
using PersonalFinanceApi.Interfaces;
using PersonalFinanceApi.Models;

namespace PersonalFinanceApi.Repositories
{
    public class AccountRepository : RepositoryBase<Account>, IAccountRepository
    {
        public AccountRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Account>> GetByTypeAsync(AccountType type)
        {
            return await _dbSet
                .Where(a => a.Type == type)
                .OrderBy(a => a.Name)
                .ToListAsync();
        }

        public async Task<Account> GetByNameAsync(string name)
        {
            return await _dbSet
                .FirstOrDefaultAsync(a => a.Name.ToLower() == name.ToLower());
        }

        public async Task<decimal> GetTotalBalanceAsync()
        {
            return await _dbSet.SumAsync(a => a.Balance);
        }

        public async Task<bool> AccountHasTransactionsAsync(int accountId)
        {
            return await _context.Transactions
                .AnyAsync(t => t.AccountId == accountId);
        }

        public async Task UpdateAccountBalanceAsync(int accountId, decimal amount, TransactionType transactionType)
        {
            var account = await GetByIdAsync(accountId);
            if (account != null)
            {
                if (transactionType == TransactionType.Income)
                    account.Balance += amount;
                else
                    account.Balance -= amount;

                Update(account);
                await _context.SaveChangesAsync();
            }
        }

        public override async Task<IEnumerable<Account>> GetAllAsync()
        {
            return await _dbSet
                .OrderBy(a => a.Type)
                .ThenBy(a => a.Name)
                .ToListAsync();
        }
    }
}
using Microsoft.EntityFrameworkCore;
using PersonalFinanceApi.Data;
using PersonalFinanceApi.Interfaces;
using PersonalFinanceApi.Models;

namespace PersonalFinanceApi.Repositories
{
    public class CategoryRepository : RepositoryBase<Category>, ICategoryRepository
    {
        public CategoryRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Category>> GetIncomeCategoriesAsync()
        {
            return await _dbSet
                .Where(c => c.Type == TransactionType.Income)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetExpenseCategoriesAsync()
        {
            return await _dbSet
                .Where(c => c.Type == TransactionType.Expense)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Category> GetByNameAsync(string name)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());
        }

        public async Task<bool> CategoryHasTransactionsAsync(int categoryId)
        {
            return await _context.Transactions
                .AnyAsync(t => t.CategoryId == categoryId);
        }
    }
}
using PersonalFinanceApi.Models;

namespace PersonalFinanceApi.Interfaces
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<IEnumerable<Category>> GetIncomeCategoriesAsync();
        Task<IEnumerable<Category>> GetExpenseCategoriesAsync();
        Task<Category> GetByNameAsync(string name);
        Task<bool> CategoryHasTransactionsAsync(int categoryId);
    }
}
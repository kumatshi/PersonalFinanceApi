using PersonalFinanceApi.DTOs;

namespace PersonalFinanceApi.Interfaces
{
    public interface ITransactionService
    {
        /// <summary>
        /// Получить все транзакции с пагинацией
        /// </summary>
        Task<ApiResponse<IEnumerable<TransactionDto>>> GetAllTransactionsAsync(int page = 1, int pageSize = 10);

        /// <summary>
        /// Получить транзакцию по ID
        /// </summary>
        Task<ApiResponse<TransactionDto>> GetTransactionByIdAsync(int id);

        /// <summary>
        /// Создать новую транзакцию
        /// </summary>
        Task<ApiResponse<TransactionDto>> CreateTransactionAsync(CreateTransactionDto createDto);

        /// <summary>
        /// Обновить существующую транзакцию
        /// </summary>
        Task<ApiResponse<TransactionDto>> UpdateTransactionAsync(int id, UpdateTransactionDto updateDto);

        /// <summary>
        /// Удалить транзакцию
        /// </summary>
        Task<ApiResponse<bool>> DeleteTransactionAsync(int id);

        /// <summary>
        /// Получить транзакции по типу (доход/расход)
        /// </summary>
        Task<ApiResponse<IEnumerable<TransactionDto>>> GetTransactionsByTypeAsync(int type, int page = 1, int pageSize = 10);

        /// <summary>
        /// Получить транзакции по счету
        /// </summary>
        Task<ApiResponse<IEnumerable<TransactionDto>>> GetTransactionsByAccountAsync(int accountId, int page = 1, int pageSize = 10);

        /// <summary>
        /// Получить транзакции по категории
        /// </summary>
        Task<ApiResponse<IEnumerable<TransactionDto>>> GetTransactionsByCategoryAsync(int categoryId, int page = 1, int pageSize = 10);

        /// <summary>
        /// Получить финансовую сводку за период
        /// </summary>
        Task<ApiResponse<FinancialSummaryDto>> GetFinancialSummaryAsync(DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Получить распределение расходов по категориям
        /// </summary>
        Task<ApiResponse<IEnumerable<CategorySummaryDto>>> GetExpensesByCategoryAsync(DateTime? startDate = null, DateTime? endDate = null);
    }
}
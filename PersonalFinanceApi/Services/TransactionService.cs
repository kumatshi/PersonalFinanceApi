using AutoMapper;
using PersonalFinanceApi.DTOs;
using PersonalFinanceApi.Interfaces;
using PersonalFinanceApi.Models;

namespace PersonalFinanceApi.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IMapper _mapper;

        public TransactionService(
            ITransactionRepository transactionRepository,
            ICategoryRepository categoryRepository,
            IAccountRepository accountRepository,
            IMapper mapper)
        {
            _transactionRepository = transactionRepository;
            _categoryRepository = categoryRepository;
            _accountRepository = accountRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Получить все транзакции с пагинацией
        /// </summary>
        public async Task<ApiResponse<IEnumerable<TransactionDto>>> GetAllTransactionsAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var transactions = await _transactionRepository.GetTransactionsWithDetailsAsync();
                var pagedTransactions = transactions
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var transactionDtos = _mapper.Map<IEnumerable<TransactionDto>>(pagedTransactions);

                return new ApiResponse<IEnumerable<TransactionDto>>
                {
                    Success = true,
                    Message = "Транзакции успешно получены",
                    Data = transactionDtos
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при получении транзакций: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Получить транзакцию по ID
        /// </summary>
        public async Task<ApiResponse<TransactionDto>> GetTransactionByIdAsync(int id)
        {
            try
            {
                var transaction = await _transactionRepository.GetByIdAsync(id);
                if (transaction == null)
                {
                    return new ApiResponse<TransactionDto>
                    {
                        Success = false,
                        Message = $"Транзакция с ID {id} не найдена"
                    };
                }

                var transactionDto = _mapper.Map<TransactionDto>(transaction);

                return new ApiResponse<TransactionDto>
                {
                    Success = true,
                    Message = "Транзакция успешно получена",
                    Data = transactionDto
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при получении транзакции: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Создать новую транзакцию
        /// </summary>
        public async Task<ApiResponse<TransactionDto>> CreateTransactionAsync(CreateTransactionDto createDto)
        {
            try
            {
                if (createDto.Amount <= 0)
                {
                    return new ApiResponse<TransactionDto>
                    {
                        Success = false,
                        Message = "Сумма транзакции должна быть больше 0"
                    };
                }

                var categoryExists = await _categoryRepository.ExistsAsync(createDto.CategoryId);
                if (!categoryExists)
                {
                    return new ApiResponse<TransactionDto>
                    {
                        Success = false,
                        Message = "Указанная категория не существует"
                    };
                }

                var accountExists = await _accountRepository.ExistsAsync(createDto.AccountId);
                if (!accountExists)
                {
                    return new ApiResponse<TransactionDto>
                    {
                        Success = false,
                        Message = "Указанный счет не существует"
                    };
                }

                var transaction = _mapper.Map<Transaction>(createDto);

                if (transaction.Date == DateTime.MinValue)
                    transaction.Date = DateTime.UtcNow;

                await _transactionRepository.AddAsync(transaction);
                await _transactionRepository.SaveChangesAsync();

                await _accountRepository.UpdateAccountBalanceAsync(
                    transaction.AccountId,
                    transaction.Amount,
                    (TransactionType)createDto.Type);

                var createdTransaction = await _transactionRepository.GetByIdAsync(transaction.Id);
                var transactionDto = _mapper.Map<TransactionDto>(createdTransaction);

                return new ApiResponse<TransactionDto>
                {
                    Success = true,
                    Message = "Транзакция успешно создана",
                    Data = transactionDto
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при создании транзакции: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Обновить существующую транзакцию
        /// </summary>
        public async Task<ApiResponse<TransactionDto>> UpdateTransactionAsync(int id, UpdateTransactionDto updateDto)
        {
            try
            {
                var existingTransaction = await _transactionRepository.GetByIdAsync(id);
                if (existingTransaction == null)
                {
                    return new ApiResponse<TransactionDto>
                    {
                        Success = false,
                        Message = $"Транзакция с ID {id} не найдена"
                    };
                }

                var oldAmount = existingTransaction.Amount;
                var oldType = existingTransaction.Type;
                var oldAccountId = existingTransaction.AccountId;

                await _accountRepository.UpdateAccountBalanceAsync(oldAccountId, -oldAmount, oldType);

                _mapper.Map(updateDto, existingTransaction);
                _transactionRepository.Update(existingTransaction);
                await _transactionRepository.SaveChangesAsync();

                await _accountRepository.UpdateAccountBalanceAsync(
                    existingTransaction.AccountId,
                    existingTransaction.Amount,
                    existingTransaction.Type);

                var updatedTransaction = await _transactionRepository.GetByIdAsync(id);
                var transactionDto = _mapper.Map<TransactionDto>(updatedTransaction);

                return new ApiResponse<TransactionDto>
                {
                    Success = true,
                    Message = "Транзакция успешно обновлена",
                    Data = transactionDto
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при обновлении транзакции: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Удалить транзакцию
        /// </summary>
        public async Task<ApiResponse<bool>> DeleteTransactionAsync(int id)
        {
            try
            {
                var transaction = await _transactionRepository.GetByIdAsync(id);
                if (transaction == null)
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Message = $"Транзакция с ID {id} не найдена"
                    };
                }

                await _accountRepository.UpdateAccountBalanceAsync(
                    transaction.AccountId,
                    -transaction.Amount,
                    transaction.Type);

                _transactionRepository.Remove(transaction);
                await _transactionRepository.SaveChangesAsync();

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Транзакция успешно удалена",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при удалении транзакции: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Получить транзакции по типу (доход/расход)
        /// </summary>
        public async Task<ApiResponse<IEnumerable<TransactionDto>>> GetTransactionsByTypeAsync(int type, int page = 1, int pageSize = 10)
        {
            try
            {
                var transactionType = (TransactionType)type;
                var transactions = await _transactionRepository.GetByTypeAsync(transactionType);
                var pagedTransactions = transactions
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var transactionDtos = _mapper.Map<IEnumerable<TransactionDto>>(pagedTransactions);

                return new ApiResponse<IEnumerable<TransactionDto>>
                {
                    Success = true,
                    Message = $"Транзакции типа {transactionType} успешно получены",
                    Data = transactionDtos
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при получении транзакций по типу: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Получить транзакции по счету
        /// </summary>
        public async Task<ApiResponse<IEnumerable<TransactionDto>>> GetTransactionsByAccountAsync(int accountId, int page = 1, int pageSize = 10)
        {
            try
            {
                var transactions = await _transactionRepository.GetByAccountIdAsync(accountId);
                var pagedTransactions = transactions
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var transactionDtos = _mapper.Map<IEnumerable<TransactionDto>>(pagedTransactions);

                return new ApiResponse<IEnumerable<TransactionDto>>
                {
                    Success = true,
                    Message = "Транзакции по счету успешно получены",
                    Data = transactionDtos
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при получении транзакций по счету: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Получить транзакции по категории
        /// </summary>
        public async Task<ApiResponse<IEnumerable<TransactionDto>>> GetTransactionsByCategoryAsync(int categoryId, int page = 1, int pageSize = 10)
        {
            try
            {
                var transactions = await _transactionRepository.GetByCategoryIdAsync(categoryId);
                var pagedTransactions = transactions
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var transactionDtos = _mapper.Map<IEnumerable<TransactionDto>>(pagedTransactions);

                return new ApiResponse<IEnumerable<TransactionDto>>
                {
                    Success = true,
                    Message = "Транзакции по категории успешно получены",
                    Data = transactionDtos
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при получении транзакций по категории: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Получить финансовую сводку за период
        /// </summary>
        public async Task<ApiResponse<FinancialSummaryDto>> GetFinancialSummaryAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                startDate ??= DateTime.UtcNow.AddMonths(-1);
                endDate ??= DateTime.UtcNow;

                var totalIncome = await _transactionRepository.GetTotalIncomeAsync(startDate, endDate);
                var totalExpenses = await _transactionRepository.GetTotalExpensesAsync(startDate, endDate);
                var balance = totalIncome - totalExpenses;
                var savingsRate = totalIncome > 0 ? (balance / totalIncome) * 100 : 0;

                var summary = new FinancialSummaryDto
                {
                    TotalIncome = totalIncome,
                    TotalExpenses = totalExpenses,
                    Balance = balance,
                    SavingsRate = Math.Round(savingsRate, 2),
                    TotalTransactions = await _transactionRepository.CountAsync(),
                    PeriodStart = startDate.Value,
                    PeriodEnd = endDate.Value
                };

                return new ApiResponse<FinancialSummaryDto>
                {
                    Success = true,
                    Message = "Финансовая сводка успешно получена",
                    Data = summary
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при получении финансовой сводки: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Получить распределение расходов по категориям
        /// </summary>
        public async Task<ApiResponse<IEnumerable<CategorySummaryDto>>> GetExpensesByCategoryAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                startDate ??= DateTime.UtcNow.AddMonths(-1);
                endDate ??= DateTime.UtcNow;

                var expenses = await _transactionRepository.GetByTypeAsync(TransactionType.Expense);
                var periodExpenses = expenses
                    .Where(t => t.Date >= startDate && t.Date <= endDate)
                    .ToList();

                var totalExpenses = periodExpenses.Sum(t => t.Amount);

                var categorySummaries = periodExpenses
                    .GroupBy(t => new { t.Category.Name, t.Category.Color })
                    .Select(g => new CategorySummaryDto
                    {
                        CategoryName = g.Key.Name,
                        CategoryColor = g.Key.Color,
                        TotalAmount = g.Sum(t => t.Amount),
                        Percentage = totalExpenses > 0 ? Math.Round((g.Sum(t => t.Amount) / totalExpenses) * 100, 2) : 0,
                        TransactionCount = g.Count()
                    })
                    .OrderByDescending(c => c.TotalAmount)
                    .ToList();

                return new ApiResponse<IEnumerable<CategorySummaryDto>>
                {
                    Success = true,
                    Message = "Распределение расходов по категориям успешно получено",
                    Data = categorySummaries
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при получении распределения расходов: {ex.Message}", ex);
            }
        }
        public async Task<bool> UserOwnsTransactionAsync(int transactionId, int userId)
        {
            var transaction = await _transactionRepository.GetByIdAsync(transactionId);
            return transaction?.UserId == userId;
        }

        public async Task<bool> UserOwnsAccountAsync(int accountId, int userId)
        {
            var account = await _accountRepository.GetByIdAsync(accountId);
            return account?.UserId == userId;
        }
    }
}
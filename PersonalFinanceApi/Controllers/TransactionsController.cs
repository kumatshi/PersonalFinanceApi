using Microsoft.AspNetCore.Mvc;
using PersonalFinanceApi.DTOs;
using PersonalFinanceApi.Interfaces;

namespace PersonalFinanceApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionsController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        /// <summary>
        /// Получить список всех транзакций с пагинацией
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<TransactionDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<TransactionDto>>>> GetTransactions(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _transactionService.GetAllTransactionsAsync(page, pageSize);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Получить транзакцию по ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<TransactionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<TransactionDto>>> GetTransaction(int id)
        {
            var result = await _transactionService.GetTransactionByIdAsync(id);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Создать новую транзакцию
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<TransactionDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<TransactionDto>>> CreateTransaction(CreateTransactionDto createDto)
        {
            var result = await _transactionService.CreateTransactionAsync(createDto);

            if (!result.Success)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetTransaction), new { id = result.Data?.Id }, result);
        }

        /// <summary>
        /// Обновить существующую транзакцию
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<TransactionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<TransactionDto>>> UpdateTransaction(int id, UpdateTransactionDto updateDto)
        {
            var result = await _transactionService.UpdateTransactionAsync(id, updateDto);

            if (!result.Success)
            {
                if (result.Message.Contains("не найдена"))
                    return NotFound(result);
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Удалить транзакцию
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteTransaction(int id)
        {
            var result = await _transactionService.DeleteTransactionAsync(id);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Получить транзакции по типу (доход/расход)
        /// </summary>
        [HttpGet("type/{type}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<TransactionDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<TransactionDto>>>> GetTransactionsByType(
            int type,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _transactionService.GetTransactionsByTypeAsync(type, page, pageSize);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Получить транзакции по счету
        /// </summary>
        [HttpGet("account/{accountId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<TransactionDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<TransactionDto>>>> GetTransactionsByAccount(
            int accountId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _transactionService.GetTransactionsByAccountAsync(accountId, page, pageSize);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Получить транзакции по категории
        /// </summary>
        [HttpGet("category/{categoryId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<TransactionDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<TransactionDto>>>> GetTransactionsByCategory(
            int categoryId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _transactionService.GetTransactionsByCategoryAsync(categoryId, page, pageSize);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Получить финансовую сводку за период
        /// </summary>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<FinancialSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FinancialSummaryDto>>> GetFinancialSummary(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var result = await _transactionService.GetFinancialSummaryAsync(startDate, endDate);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Получить распределение расходов по категориям
        /// </summary>
        [HttpGet("expenses-by-category")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategorySummaryDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<CategorySummaryDto>>>> GetExpensesByCategory(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var result = await _transactionService.GetExpensesByCategoryAsync(startDate, endDate);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
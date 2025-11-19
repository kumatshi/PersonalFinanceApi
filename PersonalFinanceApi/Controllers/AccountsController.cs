using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceApi.DTOs;
using PersonalFinanceApi.Interfaces;
using PersonalFinanceApi.Models;

namespace PersonalFinanceApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IMapper _mapper;

        public AccountsController(IAccountRepository accountRepository, IMapper mapper)
        {
            _accountRepository = accountRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Получить список всех счетов (доступно всем аутентифицированным пользователям)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<AccountDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<AccountDto>>>> GetAccounts()
        {
            try
            {
                var accounts = await _accountRepository.GetAllAsync();
                var accountDtos = _mapper.Map<IEnumerable<AccountDto>>(accounts);
                foreach (var accountDto in accountDtos)
                {
                    accountDto.TransactionCount = await _accountRepository.CountAsync(
                        a => a.Id == accountDto.Id);
                }

                var response = new ApiResponse<IEnumerable<AccountDto>>
                {
                    Success = true,
                    Message = "Счета успешно получены",
                    Data = accountDtos
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ErrorResponse
                {
                    Success = false,
                    Message = "Ошибка при получении счетов",
                    ErrorCode = "ACCOUNTS_GET_ERROR"
                };

                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// Получить счет по ID (доступно всем аутентифицированным пользователям)
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<AccountDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<AccountDto>>> GetAccount(int id)
        {
            try
            {
                var account = await _accountRepository.GetByIdAsync(id);
                if (account == null)
                {
                    var notFoundResponse = new ApiResponse<AccountDto>
                    {
                        Success = false,
                        Message = $"Счет с ID {id} не найден"
                    };
                    return NotFound(notFoundResponse);
                }

                var accountDto = _mapper.Map<AccountDto>(account);
                accountDto.TransactionCount = await _accountRepository.CountAsync(a => a.Id == id);

                var response = new ApiResponse<AccountDto>
                {
                    Success = true,
                    Message = "Счет успешно получен",
                    Data = accountDto
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ErrorResponse
                {
                    Success = false,
                    Message = "Ошибка при получении счета",
                    ErrorCode = "ACCOUNT_GET_ERROR"
                };

                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// Получить сводную информацию по счетам (только для Premium и Admin)
        /// </summary>
        [HttpGet("summary")]
        [Authorize(Roles = "Admin,Premium")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> GetAccountsSummary()
        {
            try
            {
                var accounts = await _accountRepository.GetAllAsync();
                var totalBalance = await _accountRepository.GetTotalBalanceAsync();

                var summary = new
                {
                    TotalBalance = totalBalance,
                    TotalAccounts = accounts.Count(),
                    AccountsByType = accounts.GroupBy(a => a.Type)
                        .Select(g => new
                        {
                            Type = g.Key.ToString(),
                            Count = g.Count(),
                            TotalBalance = g.Sum(a => a.Balance)
                        })
                };

                var response = new ApiResponse<object>
                {
                    Success = true,
                    Message = "Сводная информация по счетам успешно получена",
                    Data = summary
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ErrorResponse
                {
                    Success = false,
                    Message = "Ошибка при получении сводной информации по счетам",
                    ErrorCode = "ACCOUNTS_SUMMARY_ERROR"
                };

                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// Создать новый счет (доступно всем аутентифицированным пользователям)
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<AccountDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<AccountDto>>> CreateAccount(CreateAccountDto createDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(createDto.Name))
                {
                    var badRequestResponse = new ApiResponse<AccountDto>
                    {
                        Success = false,
                        Message = "Название счета обязательно"
                    };
                    return BadRequest(badRequestResponse);
                }

                var account = _mapper.Map<Account>(createDto);
                await _accountRepository.AddAsync(account);
                await _accountRepository.SaveChangesAsync();

                var accountDto = _mapper.Map<AccountDto>(account);

                var response = new ApiResponse<AccountDto>
                {
                    Success = true,
                    Message = "Счет успешно создан",
                    Data = accountDto
                };

                return CreatedAtAction(nameof(GetAccount), new { id = account.Id }, response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ErrorResponse
                {
                    Success = false,
                    Message = "Ошибка при создании счета",
                    ErrorCode = "ACCOUNT_CREATE_ERROR"
                };

                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// Обновить существующий счет (только для владельца счета или Admin)
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<AccountDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<AccountDto>>> UpdateAccount(int id, UpdateAccountDto updateDto)
        {
            try
            {
                var existingAccount = await _accountRepository.GetByIdAsync(id);
                if (existingAccount == null)
                {
                    var notFoundResponse = new ApiResponse<AccountDto>
                    {
                        Success = false,
                        Message = $"Счет с ID {id} не найден"
                    };
                    return NotFound(notFoundResponse);
                }

                // Проверка прав доступа
                var currentUserId = GetCurrentUserId();
                if (existingAccount.UserId != currentUserId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                _mapper.Map(updateDto, existingAccount);
                _accountRepository.Update(existingAccount);
                await _accountRepository.SaveChangesAsync();

                var accountDto = _mapper.Map<AccountDto>(existingAccount);

                var response = new ApiResponse<AccountDto>
                {
                    Success = true,
                    Message = "Счет успешно обновлен",
                    Data = accountDto
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ErrorResponse
                {
                    Success = false,
                    Message = "Ошибка при обновлении счета",
                    ErrorCode = "ACCOUNT_UPDATE_ERROR"
                };

                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// Удалить счет (только для Admin)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteAccount(int id)
        {
            try
            {
                var account = await _accountRepository.GetByIdAsync(id);
                if (account == null)
                {
                    var notFoundResponse = new ApiResponse<bool>
                    {
                        Success = false,
                        Message = $"Счет с ID {id} не найден"
                    };
                    return NotFound(notFoundResponse);
                }
                var hasTransactions = await _accountRepository.AccountHasTransactionsAsync(id);
                if (hasTransactions)
                {
                    var conflictResponse = new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Невозможно удалить счет, так как с ним связаны транзакции"
                    };
                    return BadRequest(conflictResponse);
                }

                _accountRepository.Remove(account);
                await _accountRepository.SaveChangesAsync();

                var response = new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Счет успешно удален",
                    Data = true
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ErrorResponse
                {
                    Success = false,
                    Message = "Ошибка при удалении счета",
                    ErrorCode = "ACCOUNT_DELETE_ERROR"
                };

                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// Получить ID текущего пользователя из токена
        /// </summary>
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            return int.Parse(userIdClaim?.Value ?? "0");
        }
    }
}
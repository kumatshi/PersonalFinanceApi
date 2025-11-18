using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinanceApi.DTOs;
using PersonalFinanceApi.Interfaces;
using PersonalFinanceApi.Models;
using Microsoft.EntityFrameworkCore;

namespace PersonalFinanceApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public CategoriesController(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Получить список всех категорий
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoryDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<CategoryDto>>>> GetCategories()
        {
            try
            {
                var categories = await _categoryRepository.GetAllAsync();
                var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);

                foreach (var categoryDto in categoryDtos)
                {
                    categoryDto.TransactionCount = await _categoryRepository.CountAsync(
                        c => c.Id == categoryDto.Id);
                }

                var response = new ApiResponse<IEnumerable<CategoryDto>>
                {
                    Success = true,
                    Message = "Категории успешно получены",
                    Data = categoryDtos
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ErrorResponse
                {
                    Success = false,
                    Message = "Ошибка при получении категорий",
                    ErrorCode = "CATEGORIES_GET_ERROR"
                };

                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// Получить категорию по ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> GetCategory(int id)
        {
            try
            {
                var category = await _categoryRepository.GetByIdAsync(id);
                if (category == null)
                {
                    var notFoundResponse = new ApiResponse<CategoryDto>
                    {
                        Success = false,
                        Message = $"Категория с ID {id} не найдена"
                    };
                    return NotFound(notFoundResponse);
                }

                var categoryDto = _mapper.Map<CategoryDto>(category);
                categoryDto.TransactionCount = await _categoryRepository.CountAsync(c => c.Id == id);

                var response = new ApiResponse<CategoryDto>
                {
                    Success = true,
                    Message = "Категория успешно получена",
                    Data = categoryDto
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ErrorResponse
                {
                    Success = false,
                    Message = "Ошибка при получении категории",
                    ErrorCode = "CATEGORY_GET_ERROR"
                };

                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// Получить категории доходов
        /// </summary>
        [HttpGet("income")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoryDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<CategoryDto>>>> GetIncomeCategories()
        {
            try
            {
                var categories = await _categoryRepository.GetIncomeCategoriesAsync();
                var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);

                var response = new ApiResponse<IEnumerable<CategoryDto>>
                {
                    Success = true,
                    Message = "Категории доходов успешно получены",
                    Data = categoryDtos
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ErrorResponse
                {
                    Success = false,
                    Message = "Ошибка при получении категорий доходов",
                    ErrorCode = "INCOME_CATEGORIES_GET_ERROR"
                };

                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// Получить категории расходов
        /// </summary>
        [HttpGet("expense")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoryDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<CategoryDto>>>> GetExpenseCategories()
        {
            try
            {
                var categories = await _categoryRepository.GetExpenseCategoriesAsync();
                var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);

                var response = new ApiResponse<IEnumerable<CategoryDto>>
                {
                    Success = true,
                    Message = "Категории расходов успешно получены",
                    Data = categoryDtos
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ErrorResponse
                {
                    Success = false,
                    Message = "Ошибка при получении категорий расходов",
                    ErrorCode = "EXPENSE_CATEGORIES_GET_ERROR"
                };

                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// Создать новую категорию
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> CreateCategory(CreateCategoryDto createDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(createDto.Name))
                {
                    var badRequestResponse = new ApiResponse<CategoryDto>
                    {
                        Success = false,
                        Message = "Название категории обязательно"
                    };
                    return BadRequest(badRequestResponse);
                }

                var category = _mapper.Map<Category>(createDto);
                await _categoryRepository.AddAsync(category);
                await _categoryRepository.SaveChangesAsync();

                var categoryDto = _mapper.Map<CategoryDto>(category);

                var response = new ApiResponse<CategoryDto>
                {
                    Success = true,
                    Message = "Категория успешно создана",
                    Data = categoryDto
                };

                return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ErrorResponse
                {
                    Success = false,
                    Message = "Ошибка при создании категории",
                    ErrorCode = "CATEGORY_CREATE_ERROR"
                };

                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// Обновить существующую категорию
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> UpdateCategory(int id, UpdateCategoryDto updateDto)
        {
            try
            {
                var existingCategory = await _categoryRepository.GetByIdAsync(id);
                if (existingCategory == null)
                {
                    var notFoundResponse = new ApiResponse<CategoryDto>
                    {
                        Success = false,
                        Message = $"Категория с ID {id} не найдена"
                    };
                    return NotFound(notFoundResponse);
                }

                _mapper.Map(updateDto, existingCategory);
                _categoryRepository.Update(existingCategory);
                await _categoryRepository.SaveChangesAsync();

                var categoryDto = _mapper.Map<CategoryDto>(existingCategory);

                var response = new ApiResponse<CategoryDto>
                {
                    Success = true,
                    Message = "Категория успешно обновлена",
                    Data = categoryDto
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ErrorResponse
                {
                    Success = false,
                    Message = "Ошибка при обновлении категории",
                    ErrorCode = "CATEGORY_UPDATE_ERROR"
                };

                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// Удалить категорию
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteCategory(int id)
        {
            try
            {
                var category = await _categoryRepository.GetByIdAsync(id);
                if (category == null)
                {
                    var notFoundResponse = new ApiResponse<bool>
                    {
                        Success = false,
                        Message = $"Категория с ID {id} не найдена"
                    };
                    return NotFound(notFoundResponse);
                }

                var hasTransactions = await _categoryRepository.CategoryHasTransactionsAsync(id);
                if (hasTransactions)
                {
                    var conflictResponse = new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Невозможно удалить категорию, так как с ней связаны транзакции"
                    };
                    return BadRequest(conflictResponse);
                }

                _categoryRepository.Remove(category);
                await _categoryRepository.SaveChangesAsync();

                var response = new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Категория успешно удалена",
                    Data = true
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ErrorResponse
                {
                    Success = false,
                    Message = "Ошибка при удалении категории",
                    ErrorCode = "CATEGORY_DELETE_ERROR"
                };

                return StatusCode(500, errorResponse);
            }
        }
    }
}
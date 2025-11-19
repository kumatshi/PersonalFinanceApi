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
    public class UsersController : ControllerBase
    {
        private readonly IAuthService _authService;

        public UsersController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Получить профиль текущего пользователя (доступно всем аутентифицированным пользователям)
        /// </summary>
        [HttpGet("profile")]
        [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<UserProfileDto>>> GetProfile()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new ApiResponse<UserProfileDto>
                {
                    Success = false,
                    Message = "Пользователь не авторизован"
                });

            var result = await _authService.GetUserProfileAsync(userId.Value);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Получить список всех пользователей (только для Admin)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserProfileDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<UserProfileDto>>>> GetAllUsers()
        {
            try
            {
                // В реальном приложении здесь был бы вызов сервиса для получения всех пользователей
                // Для демонстрации возвращаем заглушку
                var response = new ApiResponse<IEnumerable<UserProfileDto>>
                {
                    Success = true,
                    Message = "Список пользователей успешно получен",
                    Data = new List<UserProfileDto>()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ErrorResponse
                {
                    Success = false,
                    Message = "Ошибка при получении списка пользователей",
                    ErrorCode = "USERS_GET_ERROR"
                };

                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// Обновить роль пользователя (только для Admin)
        /// </summary>
        [HttpPut("{id}/role")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<UserProfileDto>>> UpdateUserRole(int id, [FromBody] string newRole)
        {
            try
            {
                // В реальном приложении здесь был бы вызов сервиса для обновления роли
                // Для демонстрации возвращаем заглушку
                var response = new ApiResponse<UserProfileDto>
                {
                    Success = true,
                    Message = $"Роль пользователя успешно обновлена на {newRole}",
                    Data = null
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ErrorResponse
                {
                    Success = false,
                    Message = "Ошибка при обновлении роли пользователя",
                    ErrorCode = "USER_ROLE_UPDATE_ERROR"
                };

                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// Получить статистику по пользователям (только для Admin)
        /// </summary>
        [HttpGet("statistics")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> GetUsersStatistics()
        {
            try
            {
                var statistics = new
                {
                    TotalUsers = 100,
                    ActiveUsers = 75,
                    NewUsersThisMonth = 15,
                    UsersByRole = new[]
                    {
                        new { Role = "Admin", Count = 5 },
                        new { Role = "Premium", Count = 25 },
                        new { Role = "User", Count = 70 }
                    }
                };

                var response = new ApiResponse<object>
                {
                    Success = true,
                    Message = "Статистика пользователей успешно получена",
                    Data = statistics
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ErrorResponse
                {
                    Success = false,
                    Message = "Ошибка при получении статистики пользователей",
                    ErrorCode = "USERS_STATISTICS_ERROR"
                };

                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// Получение ID текущего пользователя из токена
        /// </summary>
        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return null;

            return userId;
        }
    }
}
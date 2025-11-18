using Microsoft.AspNetCore.Mvc;
using PersonalFinanceApi.DTOs;
using PersonalFinanceApi.Interfaces;

namespace PersonalFinanceApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Регистрация нового пользователя
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register(RegisterRequestDto registerDto)
        {
            var result = await _authService.RegisterAsync(registerDto);

            if (!result.Success)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetProfile), new { id = result.Data?.UserId }, result);
        }

        /// <summary>
        /// Вход пользователя в систему
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login(LoginRequestDto loginDto)
        {
            var result = await _authService.LoginAsync(loginDto);

            if (!result.Success)
                return Unauthorized(result);

            return Ok(result);
        }

        /// <summary>
        /// Обновление JWT токена
        /// </summary>
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> RefreshToken(RefreshTokenRequestDto refreshDto)
        {
            var result = await _authService.RefreshTokenAsync(refreshDto.RefreshToken);

            if (!result.Success)
                return Unauthorized(result);

            return Ok(result);
        }

        /// <summary>
        /// Получение профиля текущего пользователя
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
        /// Выход пользователя из системы
        /// </summary>
        [HttpPost("logout")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<bool>>> Logout()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Пользователь не авторизован"
                });

            var result = await _authService.LogoutAsync(userId.Value);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
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
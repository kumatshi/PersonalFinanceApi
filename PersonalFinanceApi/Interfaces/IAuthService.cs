using PersonalFinanceApi.DTOs;

namespace PersonalFinanceApi.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterRequestDto registerDto);

        Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto loginDto);

        Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(string refreshToken);

        Task<ApiResponse<UserProfileDto>> GetUserProfileAsync(int userId);

        Task<ApiResponse<bool>> LogoutAsync(int userId);
    }
}
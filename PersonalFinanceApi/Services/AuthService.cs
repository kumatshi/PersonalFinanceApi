using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using PersonalFinanceApi.Data;
using PersonalFinanceApi.DTOs;
using PersonalFinanceApi.Interfaces;
using PersonalFinanceApi.Models;
using Microsoft.EntityFrameworkCore;

namespace PersonalFinanceApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public AuthService(AppDbContext context, IConfiguration configuration, IMapper mapper)
        {
            _context = context;
            _configuration = configuration;
            _mapper = mapper;
        }

        /// <summary>
        /// Регистрирует нового пользователя
        /// </summary>
        public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterRequestDto registerDto)
        {
            try
            {
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == registerDto.Username || u.Email == registerDto.Email);

                if (existingUser != null)
                {
                    return new ApiResponse<AuthResponseDto>
                    {
                        Success = false,
                        Message = existingUser.Username == registerDto.Username
                            ? "Пользователь с таким именем уже существует"
                            : "Пользователь с таким email уже существует"
                    };
                }
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

                var user = new User
                {
                    Username = registerDto.Username,
                    Email = registerDto.Email,
                    PasswordHash = passwordHash,
                    Role = UserRoles.User,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                var tokens = await GenerateTokensAsync(user);

                return new ApiResponse<AuthResponseDto>
                {
                    Success = true,
                    Message = "Пользователь успешно зарегистрирован",
                    Data = tokens
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при регистрации пользователя: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Выполняет вход пользователя
        /// </summary>
        public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginRequestDto loginDto)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == loginDto.UsernameOrEmail || u.Email == loginDto.UsernameOrEmail);

                if (user == null)
                {
                    return new ApiResponse<AuthResponseDto>
                    {
                        Success = false,
                        Message = "Неверное имя пользователя/email или пароль"
                    };
                }

                if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                {
                    return new ApiResponse<AuthResponseDto>
                    {
                        Success = false,
                        Message = "Неверное имя пользователя/email или пароль"
                    };
                }

                var tokens = await GenerateTokensAsync(user);

                return new ApiResponse<AuthResponseDto>
                {
                    Success = true,
                    Message = "Вход выполнен успешно",
                    Data = tokens
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при входе пользователя: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Обновляет access token с помощью refresh token
        /// </summary>
        public async Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(string refreshToken)
        {
            try
            {

                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtSettings = _configuration.GetSection("Jwt");

                try
                {
                    var validationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"])),
                        ValidateIssuer = true,
                        ValidIssuer = jwtSettings["Issuer"],
                        ValidateAudience = true,
                        ValidAudience = jwtSettings["Audience"],
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };

                    var principal = tokenHandler.ValidateToken(refreshToken, validationParameters, out _);
                    var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                    var user = await _context.Users.FindAsync(userId);
                    if (user == null)
                    {
                        return new ApiResponse<AuthResponseDto>
                        {
                            Success = false,
                            Message = "Пользователь не найден"
                        };
                    }

                    var tokens = await GenerateTokensAsync(user);

                    return new ApiResponse<AuthResponseDto>
                    {
                        Success = true,
                        Message = "Токен успешно обновлен",
                        Data = tokens
                    };
                }
                catch (SecurityTokenException)
                {
                    return new ApiResponse<AuthResponseDto>
                    {
                        Success = false,
                        Message = "Недействительный refresh token"
                    };
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при обновлении токена: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Получает профиль текущего пользователя
        /// </summary>
        public async Task<ApiResponse<UserProfileDto>> GetUserProfileAsync(int userId)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Transactions)
                    .Include(u => u.Accounts)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return new ApiResponse<UserProfileDto>
                    {
                        Success = false,
                        Message = "Пользователь не найден"
                    };
                }

                var userProfile = _mapper.Map<UserProfileDto>(user);
                userProfile.TransactionCount = user.Transactions.Count;
                userProfile.AccountCount = user.Accounts.Count;

                return new ApiResponse<UserProfileDto>
                {
                    Success = true,
                    Message = "Профиль пользователя успешно получен",
                    Data = userProfile
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при получении профиля пользователя: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Выполняет выход пользователя (инвалидирует refresh token)
        /// </summary>
        public async Task<ApiResponse<bool>> LogoutAsync(int userId)
        {
            try
            {

                return new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Выход выполнен успешно",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при выходе пользователя: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Генерирует JWT токены для пользователя
        /// </summary>
        private async Task<AuthResponseDto> GenerateTokensAsync(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var secretKeyString = jwtSettings["Secret"];

            if (string.IsNullOrEmpty(secretKeyString) || secretKeyString.Length < 32)
            {
                throw new ArgumentException("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()_-+=");
            }

            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKeyString));
            var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role)
    };

            var expiresInMinutes = int.Parse(jwtSettings["ExpiresInMinutes"] ?? "60");
            var expiresInDays = int.Parse(jwtSettings["RefreshTokenExpiresInDays"] ?? "7");

            var accessTokenExpiration = DateTime.UtcNow.AddMinutes(expiresInMinutes);
            var refreshTokenExpiration = DateTime.UtcNow.AddDays(expiresInDays);

            var accessToken = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: accessTokenExpiration,
                signingCredentials: signingCredentials
            );

            var refreshToken = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: new[]
                {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                },
                expires: refreshTokenExpiration,
                signingCredentials: signingCredentials
            );

            var tokenHandler = new JwtSecurityTokenHandler();

            return new AuthResponseDto
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                AccessToken = tokenHandler.WriteToken(accessToken),
                RefreshToken = tokenHandler.WriteToken(refreshToken),
                ExpiresAt = accessTokenExpiration
            };
        }
    }
}
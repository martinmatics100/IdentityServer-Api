
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SchoolPilot_IdentityServer.Common.Enums;
using SchoolPilot_IdentityServer.Data.Context;
using SchoolPilot_IdentityServer.Data.Contracts;
using SchoolPilot_IdentityServer.Data.Entities;
using SchoolPilot_IdentityServer.Data.Helpers;
using SchoolPilot_IdentityServer.Infrastructure.Interfaces;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace SchoolPilot_IdentityServer.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly ITokenService _tokenService;
        private readonly ICurrentUserService _currentUserService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;
        private readonly ApplicationDbContext _context;

        public UserService(ITokenService tokenService, ICurrentUserService currentUserService, UserManager<ApplicationUser> userManager, IMapper mapper, ILogger<UserService> logger, ApplicationDbContext context)
        {
            _tokenService = tokenService;
            _currentUserService = currentUserService;
            _userManager = userManager;
            _mapper = mapper;
            _logger = logger;
            _context = context;
        }

        public async Task<UserResponse> RegisterAsync(UserRegisterRequest request)
        {
            try
            {
                _logger.LogInformation("Registering user");
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    _logger.LogError("Email already exists");
                    throw new Exception("Email already exists");
                }

                var newAccountForRegisteredUser = new Account
                {
                    Id = SequentialGuid.Create(),
                    Name = "Personal Account",
                    Disabled = false,
                    CreatedOn = DateTime.UtcNow,
                    ModifiedOn = DateTime.UtcNow,
                    Flags = AccountMiscFlags.IsPersonalAccount,
                };

                var newUser = _mapper.Map<ApplicationUser>(request);
                newUser.Id = SequentialGuid.Create();
                newUser.UserName = GenerateUserName(request.FirstName, request.LastName);
                newUser.Role = UserRole.PROVIDERS;
                var result = await _userManager.CreateAsync(newUser, request.Password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to create user: {errors}", errors);
                    throw new Exception($"Failed to create user: {errors}");
                }

                var addToRoleResult = await _userManager.AddToRoleAsync(newUser, UserRole.PROVIDERS.ToString());
                if (!addToRoleResult.Succeeded)
                {
                    var roleErrors = string.Join(", ", addToRoleResult.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to add role to user: {roleErrors}", roleErrors);
                    throw new Exception($"Failed to add role to user: {roleErrors}");
                }

                _logger.LogInformation("User created successfully");
                await _tokenService.GenerateToken(newUser);

                await _context.Accounts.AddAsync(newAccountForRegisteredUser);
                await _context.SaveChangesAsync();
                newUser.AccountId = newAccountForRegisteredUser.Id;
                newUser.CreatedOn = DateTime.Now;
                newUser.ModifiedOn = DateTime.Now;
                await _context.SaveChangesAsync();
                return _mapper.Map<UserResponse>(newUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while registering the user.");
                throw;
            }

        }

        private string GenerateUserName(string firstName, string lastName)
        {
            var baseUsername = $"{firstName}{lastName}".ToLower();

            // Check if the username already exists
            var username = baseUsername;
            var count = 1;
            while (_userManager.Users.Any(u => u.UserName == username))
            {
                username = $"{baseUsername}{count}";
                count++;
            }
            return username;
        }

        public async Task<AccessTokenResponse> LoginAsync(UserLoginRequest request)
        {
            if (request == null)
            {
                _logger.LogError("Login request is null");
                throw new ArgumentNullException(nameof(request));
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password) || user.IsDeprecated)
            {
                _logger.LogError("Invalid email or password");
                throw new Exception("Invalid email or password");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var primaryRole = roles.FirstOrDefault();

            user.Role = Enum.Parse<UserRole>(primaryRole ?? "USER");

            var token = await _tokenService.GenerateToken(user);

            var refreshToken = _tokenService.GenerateRefreshToken();

            using var sha256 = SHA256.Create();
            var refreshTokenHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(refreshToken));
            user.RefreshToken = Convert.ToBase64String(refreshTokenHash);
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(2);

            user.CreatedOn = DateTime.Now;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Failed to update user: {errors}", errors);
                throw new Exception($"Failed to update user: {errors}");
            }

            var accessTokenResponse = new AccessTokenResponse
            {
                AccessToken = token,
                Role = user.Role,
                ExpiresIn = DateTime.Now.AddHours(24)
            };

            return accessTokenResponse;

        }

        public async Task<UserResponse> GetByIdAsync(Guid id)
        {
            _logger.LogInformation("Getting user by id");
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null || user.IsDeprecated)
            {
                _logger.LogError("User not found");
                throw new Exception("User not found");
            }
            _logger.LogInformation("User found");
            return _mapper.Map<UserResponse>(user);
        }

        public async Task<CurrentUserResponse> GetCurrentUserAsync()
        {
            var userId = _currentUserService.GetUserId()?.ToString();

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogError("User ID is null or empty");
                throw new Exception("Invalid user ID");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogError("User not found");
                throw new Exception("User not found");
            }
            return _mapper.Map<CurrentUserResponse>(user);
        }

        public async Task<CurrentUserResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            _logger.LogInformation("RefreshToken");

            using var sha256 = SHA256.Create();
            var refreshTokenHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(request.RefreshToken));
            var hashedRefreshToken = Convert.ToBase64String(refreshTokenHash);

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == hashedRefreshToken);
            if (user == null)
            {
                _logger.LogError("Invalid refresh token");
                throw new Exception("Invalid refresh token");
            }

            if (user.RefreshTokenExpiryTime < DateTime.Now)
            {
                _logger.LogWarning("Refresh token expired for user ID: {UserId}", user.Id);
                throw new Exception("Refresh token expired");
            }

            var newAccessToken = await _tokenService.GenerateToken(user);
            _logger.LogInformation("Access token generated successfully");
            var currentUserResponse = _mapper.Map<CurrentUserResponse>(user);
            currentUserResponse.AccessToken = newAccessToken;
            return currentUserResponse;
        }

        public async Task<RevokeRefreshTokenResponse> RevokeRefreshToken(RefreshTokenRequest refreshTokenRemoveRequest)
        {
            _logger.LogInformation("Revoking refresh token");

            try
            {
                using var sha256 = SHA256.Create();
                var refreshTokenHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(refreshTokenRemoveRequest.RefreshToken));
                var hashedRefreshToken = Convert.ToBase64String(refreshTokenHash);

                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == hashedRefreshToken);
                if (user == null)
                {
                    _logger.LogError("Invalid refresh token");
                    throw new Exception("Invalid refresh token");
                }

                if (user.RefreshTokenExpiryTime < DateTime.Now)
                {
                    _logger.LogWarning("Refresh token expired for user ID: {UserId}", user.Id);
                    throw new Exception("Refresh token expired");
                }

                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    _logger.LogError("Failed to update user");
                    return new RevokeRefreshTokenResponse
                    {
                        Message = "Failed to revoke refresh token"
                    };
                }
                _logger.LogInformation("Refresh token revoked successfully");
                return new RevokeRefreshTokenResponse
                {
                    Message = "Refresh token revoked successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to revoke refresh token: {ex}", ex.Message);
                throw new Exception("Failed to revoke refresh token");
            }
        }

        public async Task<UserResponse> UpdateAsync(Guid id, UpdateUserRequest request)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                _logger.LogError("User not found");
                throw new Exception("User not found");
            }

            user.ModifiedOn = DateTime.Now;
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Email = request.Email;
            user.Gender = request.Gender;

            await _userManager.UpdateAsync(user);
            return _mapper.Map<UserResponse>(user);
        }

        public async Task DeleteAsync(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                _logger.LogError("User not found");
                throw new Exception("User not found");
            }

            //user.IsDeprecated = true;

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                _logger.LogError($"Failed to deprecate user {id}. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                throw new Exception("Failed to deprecate user.");
            }

            _logger.LogInformation($"User with ID {id} marked as deprecated.");
        }
    }
}

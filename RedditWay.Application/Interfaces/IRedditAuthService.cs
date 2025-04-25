using RedditWay.Domain;
using RedditWay.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedditWay.Application.Interfaces
{
    public interface IRedditAuthService
    {
        Task<string> RegisterAsync(RegisterDto model);

        Task<(string token, string userId, bool requires2FA)> LoginAsync(LoginDto model);

        Task<string> UpdateUserAsync(UserDto userUpdateDto);

        Task<UserDto> GetUserByIdAsync(string userId);

        Task<string> GenerateTwoFactorTokenAsync(ApplicationUser user);

        Task<bool> VerifyTwoFactorTokenAsync(ApplicationUser user, string token);

        Task SendTwoFactorCodeAsync(ApplicationUser user);

        Task SendEmailAsync(string email, string subject, string message);

        Task<string> VerifyTwoFactorCodeAsync(string userId, string token);

        Task<string> ForgotPasswordAsync(string email);

        Task<string> ResetPasswordAsync(ResetPasswordDto model);
    }
}

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RedditWay.Application.Constants;
using RedditWay.Application.Interfaces;
using RedditWay.Domain;
using RedditWay.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace RedditWay.Infrastructure.Services
{
    public class RedditAuthService : IRedditAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public RedditAuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _context = context;
        }

        // Existing RegisterAsync method remains unchanged
        public async Task<string> RegisterAsync(RegisterDto model)
        {
            var user = new ApplicationUser { UserName = model.UserName.Replace(" ", "_"), Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.SetTwoFactorEnabledAsync(user, true);
                return "User registered successfully";
            }

            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new Exception(errors);
        }

        // Updated LoginAsync method to handle 2FA
        public async Task<(string token, string userId, bool requires2FA)> LoginAsync(LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var token = GenerateJwtToken(user);

                // Check if 2FA is enabled for the user
                if (await _userManager.GetTwoFactorEnabledAsync(user))
                {
                    // Generate and send the 2FA code via email
                    await SendTwoFactorCodeAsync(user);

                    return (token, user.Id, true); // Token is not issued yet
                }

                return (token, user.Id, false);
            }

            throw new Exception("Invalid login attempt.");
        }

        // Existing GenerateJwtToken method remains unchanged
        private string GenerateJwtToken(ApplicationUser user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Existing UpdateUserAsync method remains unchanged
        public async Task<string> UpdateUserAsync(UserDto userUpdateDto)
        {
            var user = await _userManager.FindByIdAsync(userUpdateDto.Id);
            if (user == null)
            {
                return "User not found.";
            }

            var passwordCheck = await _signInManager.CheckPasswordSignInAsync(user, userUpdateDto.CurrentPassword, lockoutOnFailure: false);
            if (!passwordCheck.Succeeded)
            {
                return "Current password is incorrect.";
            }

            if (user.Email != userUpdateDto.NewEmail)
            {
                var emailUpdateResult = await _userManager.SetEmailAsync(user, userUpdateDto.NewEmail);
                if (!emailUpdateResult.Succeeded)
                {
                    var errors = string.Join(", ", emailUpdateResult.Errors.Select(e => e.Description));
                    return errors;
                }
            }

            if (!string.IsNullOrWhiteSpace(userUpdateDto.NewPassword))
            {
                var passwordChangeResult = await _userManager.ChangePasswordAsync(user, userUpdateDto.CurrentPassword, userUpdateDto.NewPassword);
                if (!passwordChangeResult.Succeeded)
                {
                    var errors = string.Join(", ", passwordChangeResult.Errors.Select(e => e.Description));
                    return errors;
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            return "User details updated successfully.";
        }

        // Existing GetUserByIdAsync method remains unchanged
        public async Task<UserDto> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            return new UserDto
            {
                UserName = user.UserName,
                NewEmail = user.Email // Include email if needed
            };
        }

        // Existing methods for 2FA
        public async Task<string> GenerateTwoFactorTokenAsync(ApplicationUser user)
        {
            return await _userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider);
        }

        public async Task<bool> VerifyTwoFactorTokenAsync(ApplicationUser user, string token)
        {
            return await _userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider, token);
        }

        // Existing SendEmailAsync method remains unchanged
        public async Task SendEmailAsync(string email, string subject, string message)
        {
            try
            {
                var company = await _context.AppInfo.FirstAsync();

                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(company.EmailAddress); // Replace with your sender email
                    mail.To.Add(email);
                    mail.Subject = subject;
                    mail.Body = AppConstant.ConvertStringToHtml(message, 0, false);
                    mail.IsBodyHtml = true;

                    using (SmtpClient smtpServer = new SmtpClient(company.SmtpHost)) // Replace with your SMTP host
                    {
                        smtpServer.Port = company.SmtpPort; // Replace with your SMTP port
                        smtpServer.Credentials = new System.Net.NetworkCredential(company.EmailAddress, company.Password); // Replace with your credentials
                        smtpServer.EnableSsl = true;
                        smtpServer.Send(mail);
                    }
                }
            }
            catch (SmtpException ex)
            {
                throw ex;
            }
        }

        // Updated SendTwoFactorCodeAsync method remains mostly unchanged
        public async Task SendTwoFactorCodeAsync(ApplicationUser user)
        {
            var token = await GenerateTwoFactorTokenAsync(user);
            await SendEmailAsync(user.Email, "Your 2FA Code", $"Your 2FA code is: {token}");
        }

        // New method to verify 2FA code and generate JWT token
        public async Task<string> VerifyTwoFactorCodeAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            var isValid = await VerifyTwoFactorTokenAsync(user, token);
            if (!isValid)
            {
                throw new Exception("Invalid 2FA token.");
            }

            // Generate JWT token after successful 2FA verification
            var jwtToken = GenerateJwtToken(user);
            return jwtToken;
        }

        public async Task<string> ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            // Generate password reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Generate password reset link (you will need to pass this link to the frontend for the user to reset their password)
            var resetLink = $"{_configuration["App:Url"]}/reset-password?token={token}&id={user.Id}";

            // Send the password reset link to the user's email
            await SendEmailAsync(user.Email, "Password Reset", $"To reset your password, click the following link: {resetLink}");

            return "Password reset email sent.";
        }

        public async Task<string> ResetPasswordAsync(ResetPasswordDto model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            // Verify the password reset token
            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (result.Succeeded)
            {
                return "Password reset successfully.";
            }

            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new Exception(errors);
        }
    }
}

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Server.Data;
using Server.Models;
using Server.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly EmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _dbContext;

        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            EmailService emailService,
            AppDbContext dbContext,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _configuration = configuration;
            _dbContext = dbContext;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return BadRequest(new { Message = "Invalid email or password." });
            }

            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                return BadRequest(new { Message = "Email is not confirmed. Please confirm your email to log in." });
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, isPersistent: false, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                return BadRequest(new { Message = "Invalid email or password." });
            }

            // Generate JWT token
            return Ok(new { Token = GenerateJwtToken(user) });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if user with email already exists
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                if (await _userManager.IsEmailConfirmedAsync(existingUser))
                {
                    return BadRequest(new { Message = "User with this email already exists and is confirmed." });
                }
                else
                {
                    // Resend confirmation email for unconfirmed user
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(existingUser);
                    var existingUsercallbackUrl = QueryHelpers.AddQueryString(
                        $"{_configuration["Frontend:BaseUrl"]}/confirm-email",
                        new Dictionary<string, string>
                        {
                            { "userId", existingUser.Id },
                            { "token", code }
                        });

                    await _emailService.SendConfirmationEmailAsync(existingUser, existingUsercallbackUrl);
                    return Ok(new { Message = "User exists but email is not confirmed. Confirmation email resent." });
                }
            }

            var company = new Company { Id = Guid.NewGuid(), Title=model.CompanyName };
            _dbContext.Companies.Add(company);
            await _dbContext.SaveChangesAsync();

            var user = new User
            {
                UserName = model.Email, // Set UserName to Email
                Email = model.Email,
                CompanyId = company.Id
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
            }

            // Generate and send confirmation email
            var confirmationCode = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = QueryHelpers.AddQueryString(
                $"{_configuration["Frontend:BaseUrl"]}/confirm-email",
                new Dictionary<string, string>
                {
                    { "userId", user.Id },
                    { "token", confirmationCode }
                });

            await _emailService.SendConfirmationEmailAsync(user, callbackUrl);

            return Ok(new { Message = "Registration successful. Please check your email to confirm." });
        }

        [HttpPost("resend-confirmation")]
        public async Task<IActionResult> ResendConfirmation([FromBody] ResendConfirmationModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return BadRequest(new { Message = "No user found with this email." });
            }

            if (await _userManager.IsEmailConfirmedAsync(user))
            {
                return BadRequest(new { Message = "Email is already confirmed." });
            }

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = QueryHelpers.AddQueryString(
                $"{_configuration["Frontend:BaseUrl"]}/confirm-email",
                new Dictionary<string, string>
                {
                    { "userId", user.Id },
                    { "token", code }
                });

            await _emailService.SendConfirmationEmailAsync(user, callbackUrl);

            return Ok(new { Message = "Confirmation email resent successfully." });
        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return BadRequest(new { Message = "Invalid user ID." });
            }

            var result = await _userManager.ConfirmEmailAsync(user, model.Token);
            if (!result.Succeeded)
            {
                return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
            }

            return Ok(new { Message = "Email confirmed successfully." });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest model)
        {
            if (string.IsNullOrEmpty(model.RefreshToken))
            {
                return BadRequest(new { message = "Refresh token is required" });
            }

            var refreshToken = await _dbContext.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == model.RefreshToken && !rt.IsRevoked);

            if (refreshToken == null || refreshToken.ExpiresAt < DateTime.UtcNow)
            {
                return Unauthorized(new { message = "Invalid or expired refresh token" });
            }

            var user = refreshToken.User;
            var newAccessToken = GenerateJwtToken(user);
            var newRefreshToken = await GenerateRefreshToken(user, refreshToken);

            return Ok(new
            {
                Token = newAccessToken,
                RefreshToken = newRefreshToken.Token
            });
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, "User")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<RefreshToken> GenerateRefreshToken(User user, RefreshToken existingToken = null)
        {
            if (existingToken != null)
            {
                existingToken.IsRevoked = true;
                _dbContext.RefreshTokens.Update(existingToken);
            }

            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                IssuedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            _dbContext.RefreshTokens.Add(refreshToken);
            await _dbContext.SaveChangesAsync();

            return refreshToken;
        }
    }


    public class LoginModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class RegisterModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string CompanyName { get; set; }
    }

    public class ResendConfirmationModel
    {
        public string Email { get; set; }
    }

    public class ConfirmEmailModel
    {
        public string UserId { get; set; }
        public string Token { get; set; }
    }

    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; }
    }
}
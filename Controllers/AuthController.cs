using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Server.Services;
using Server.Models;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Server.DTOs;

namespace server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly EmailService _emailService;
        private readonly IConfiguration _configuration;

        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            EmailService emailService,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _configuration = configuration;
        }
        /*
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _userManager.FindByNameAsync(loginDto.Username);
            if (user == null)
                return Unauthorized("Invalid username or password.");

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!result.Succeeded)
                return Unauthorized("Invalid username or password.");

            var token = GenerateJwtToken(user);
            return Ok(new { Token = token });
        }
        */
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null)
            {
                return BadRequest(new { Message = "Invalid username or password." });
            }

            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                return BadRequest(new { Message = "Email is not confirmed. Please confirm your email to log in." });
            }

            var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, isPersistent: false, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                return BadRequest(new { Message = "Invalid username or password." });
            }

            // Generate JWT token
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            return Ok(new { Token = new JwtSecurityTokenHandler().WriteToken(token) });
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
                    var callbackUrl = QueryHelpers.AddQueryString(
                        $"{_configuration["Frontend:BaseUrl"]}/confirm-email",
                        new Dictionary<string, string>
                        {
                            { "userId", existingUser.Id },
                            { "token", code }
                        });

                    await _emailService.SendConfirmationEmailAsync(existingUser, callbackUrl);
                    return Ok(new { Message = "User exists but email is not confirmed. Confirmation email resent." });
                }
            }

            var user = new User
            {
                UserName = model.Username,
                Email = model.Email,
                CompanyName = model.CompanyName
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
            }

            // Generate and send confirmation email
            var confirmationCode = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationUrl = QueryHelpers.AddQueryString(
                $"{_configuration["Frontend:BaseUrl"]}/confirm-email",
                new Dictionary<string, string>
                {
                    { "userId", user.Id },
                    { "token", confirmationCode }
                });

            await _emailService.SendConfirmationEmailAsync(user, confirmationUrl);

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
    }

    public class RegisterModel
    {
        public string Username { get; set; }
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
}
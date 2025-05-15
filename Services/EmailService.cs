using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MimeKit;
using Server.Models;
using System.IO;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Server.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;
        private readonly IWebHostEnvironment _environment;

        public EmailService(
            IConfiguration configuration,
            UserManager<User> userManager,
            IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _userManager = userManager;
            _environment = environment;
        }

        public async Task SendConfirmationEmailAsync(User user, string callbackUrl)
        {
            // Read email template
            string templatePath = Path.Combine(
                _environment.ContentRootPath,
                _configuration["Smtp:TemplatePath"] ?? "Templates",
                "ConfirmationEmail.html");
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException("Email template not found.", templatePath);
            }

            string templateContent = await File.ReadAllTextAsync(templatePath);
            string emailBody = templateContent
                .Replace("{UserName}", user.Email) // Use Email instead of UserName
                .Replace("{CallbackUrl}", callbackUrl);

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_configuration["Smtp:FromEmail"], _configuration["Smtp:FromEmail"]));
            message.To.Add(new MailboxAddress(user.Email, user.Email));
            message.Subject = "Confirm Your Email";

            message.Body = new TextPart("html")
            {
                Text = emailBody
            };

            using var client = new SmtpClient();
            await client.ConnectAsync(_configuration["Smtp:Host"], int.Parse(_configuration["Smtp:Port"]), bool.Parse(_configuration["Smtp:UseSsl"]));

            // Perform authentication only if required
            bool requiresAuthentication = bool.Parse(_configuration["Smtp:RequiresAuthentication"] ?? "true");
            if (requiresAuthentication)
            {
                await client.AuthenticateAsync(_configuration["Smtp:Username"], _configuration["Smtp:Password"]);
            }

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
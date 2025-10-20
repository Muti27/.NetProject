using NuGet.Configuration;
using System.Net;
using System.Net.Mail;
using System.Security.Policy;

namespace Mvc.Services
{
    public interface IEmailService
    {
        Task SendEmailConfirmation(string email, string confirmUrl);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration configuration;

        public EmailService(IConfiguration config) 
        {
            configuration = config;
        }

        public async Task SendEmailConfirmation(string email, string confirmUrl)
        {
            var smtpHost = configuration["Smtp:Host"];
            var smtpPort = int.Parse(configuration["Smtp:Port"]);
            var smtpUser = configuration["Smtp:User"];
            var smtpPass = configuration["Smtp:Password"];

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true,
                UseDefaultCredentials = false
            };

            var message = new MailMessage()
            {
                From = new MailAddress(smtpUser, "系統管理員"),
                Subject = "請驗證您的電子信箱",
                Body = $"請點擊以下連結完成驗證：\n{confirmUrl}",
                IsBodyHtml = false,                
            };
            
            message.To.Add(email);

            try
            {
                await client.SendMailAsync(message);
            }
            catch (Exception e)
            {
                throw new Exception($"寄送驗證信失敗，{e}");
            }
        }
    }
}

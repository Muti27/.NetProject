using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using NuGet.Common;
using NuGet.Configuration;
using System;
using System.Buffers.Text;
using System.Net;
using System.Net.Mail;
using System.Security.Policy;

namespace Mvc.Services
{
    public interface IEmailService
    {
        Task SendEmail(string email, string token, EmailType type);
    }

    public enum EmailType
    {
        EmailVerify,
        ForgetPassword
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration configuration;
        private readonly IHttpContextAccessor httpContextAccessor;

        public EmailService(IConfiguration config, IHttpContextAccessor accessor)
        {
            configuration = config;
            httpContextAccessor = accessor;
        }

        public async Task SendEmail(string email, string token, EmailType type)
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

            //驗證url
            var request = httpContextAccessor?.HttpContext?.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            string confirmUrl = type switch
            {
                EmailType.EmailVerify => $"{baseUrl}/Auth/ConfirmEmail?email={email}&token={token}",
                EmailType.ForgetPassword => $"{baseUrl}/Auth/ResetPassword?email={email}&token={token}",
                _ => ""
            };

            string subString = string.Empty;
            string bodyString = string.Empty;

            EmailContent(type, out subString, out bodyString);

            var message = new MailMessage()
            {
                From = new MailAddress(smtpUser, "系統管理員"),
                Subject = subString,
                Body = string.Format(bodyString, confirmUrl),
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

        private void EmailContent(EmailType type, out string sub, out string body)
        {
            sub = type switch
            {
                EmailType.EmailVerify => "請驗證您的電子信箱",
                EmailType.ForgetPassword => "請重新設定密碼",
                _ => "",
            };

            body = type switch
            {
                EmailType.EmailVerify => "請點擊以下連結完成驗證：\n{0}",
                EmailType.ForgetPassword => "請點擊以下連結重新設置密碼：\n{0}",
                _ => ""
            };
        }
    }
}
